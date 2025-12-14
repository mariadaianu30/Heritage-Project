using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;
using System.Text.Json;

namespace OnlineShop.Controllers
{
    public class AIAssistantController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AIAssistantController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: /AIAssistant/Ask
        // Primește o întrebare despre un produs și returnează un răspuns
        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AIQuestionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new { error = "Întrebarea este obligatorie." });
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.FAQs)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.Status == ProductStatus.Approved);

            if (product == null)
            {
                return NotFound(new { error = "Produsul nu a fost găsit." });
            }

            string answer = await GenerateAnswer(product, request.Question);

            // Salvează întrebarea în FAQ dacă e nouă
            await SaveOrUpdateFAQ(request.ProductId, request.Question, answer);

            return Json(new AIAnswerResponse
            {
                Answer = answer,
                ProductId = request.ProductId,
                Question = request.Question
            });
        }

        // GET: /AIAssistant/FAQs/5
        // Returnează FAQ-urile pentru un produs
        [HttpGet]
        public async Task<IActionResult> FAQs(int productId)
        {
            var faqs = await _context.ProductFAQs
                .Where(f => f.ProductId == productId && f.Answer != null)
                .OrderByDescending(f => f.TimesAsked)
                .Take(5)
                .Select(f => new
                {
                    f.Question,
                    f.Answer,
                    f.TimesAsked
                })
                .ToListAsync();

            return Json(faqs);
        }

        // Generează răspuns bazat pe informațiile produsului
        private async Task<string> GenerateAnswer(Product product, string question)
        {
            string questionLower = question.ToLower();

            // Verifică dacă există deja în FAQ
            var existingFAQ = await _context.ProductFAQs
                .FirstOrDefaultAsync(f => f.ProductId == product.Id &&
                                         f.Question.ToLower().Contains(questionLower.Substring(0, Math.Min(20, questionLower.Length))));

            if (existingFAQ != null && !string.IsNullOrEmpty(existingFAQ.Answer))
            {
                // Incrementează numărul de întrebări
                existingFAQ.TimesAsked++;
                await _context.SaveChangesAsync();
                return existingFAQ.Answer;
            }

            // Răspunsuri bazate pe cuvinte cheie
            if (ContainsAny(questionLower, "preț", "pret", "cost", "costă", "costa", "lei", "ron", "bani"))
            {
                return $"Prețul pentru {product.Title} este de {product.Price:N2} RON.";
            }

            if (ContainsAny(questionLower, "stoc", "disponibil", "există", "exista", "mai aveți", "mai aveti"))
            {
                if (product.Stock > 0)
                {
                    return $"Da, avem {product.Stock} bucăți în stoc. Produsul este disponibil pentru comandă.";
                }
                else
                {
                    return "Din păcate, momentan produsul nu este în stoc. Te rugăm să verifici mai târziu.";
                }
            }

            if (ContainsAny(questionLower, "garanție", "garantie", "warranty"))
            {
                return "Toate produsele noastre beneficiază de garanție conform legislației în vigoare (24 de luni pentru produse nealimentare). Pentru detalii specifice, te rugăm să consulți specificațiile produsului sau să ne contactezi.";
            }

            if (ContainsAny(questionLower, "livrare", "transport", "primesc", "ajunge", "durează", "dureaza"))
            {
                return "Livrarea se face în 1-3 zile lucrătoare în toată țara. Costul transportului depinde de greutatea coletului și de adresa de livrare. Pentru comenzi peste 200 RON, transportul este gratuit!";
            }

            if (ContainsAny(questionLower, "retur", "returnez", "schimb", "înapoiez", "inapoiez"))
            {
                return "Ai dreptul să returnezi produsul în termen de 14 zile de la primire, fără să specifici un motiv. Produsul trebuie să fie în starea originală, cu ambalajul intact.";
            }

            if (ContainsAny(questionLower, "plată", "plata", "platesc", "plătesc", "card", "numerar", "ramburs"))
            {
                return "Acceptăm plata cu cardul online (Visa, Mastercard), transfer bancar și ramburs la livrare. Pentru plata ramburs se aplică un comision suplimentar de 10 RON.";
            }

            if (ContainsAny(questionLower, "categorie", "tip", "fel"))
            {
                return $"Acest produs face parte din categoria '{product.Category?.Name ?? "Nedefinită"}'. {product.Category?.Description ?? ""}";
            }

            if (ContainsAny(questionLower, "rating", "recenzie", "review", "părere", "parere", "nota", "notă", "stele"))
            {
                if (product.AverageRating.HasValue)
                {
                    return $"Produsul {product.Title} are un rating mediu de {product.AverageRating:N1} stele din 5, bazat pe recenziile clienților noștri.";
                }
                else
                {
                    return "Acest produs nu are încă recenzii. Fii primul care lasă o părere după achiziție!";
                }
            }

            if (ContainsAny(questionLower, "descriere", "detalii", "specificații", "specificatii", "caracteristici", "despre"))
            {
                return $"Despre {product.Title}: {product.Description}";
            }

            if (ContainsAny(questionLower, "copii", "copil", "vârstă", "varsta", "minori"))
            {
                return "Pentru informații despre compatibilitatea produsului cu diferite grupe de vârstă, te rugăm să consulți descrierea detaliată sau să ne contactezi pentru recomandări personalizate.";
            }

            if (ContainsAny(questionLower, "dimensiuni", "mărime", "marime", "size", "greutate"))
            {
                return "Pentru informații despre dimensiuni și greutate, te rugăm să consulți descrierea produsului. Dacă ai nevoie de detalii suplimentare, nu ezita să ne contactezi.";
            }

            // Răspuns default dacă nu găsim informații relevante
            return $"Momentan nu am informații specifice despre acest aspect pentru produsul {product.Title}. " +
                   $"Te pot ajuta cu întrebări despre preț, disponibilitate, garanție, livrare, retur sau specificații. " +
                   $"Pentru întrebări mai detaliate, te rugăm să ne contactezi direct.";
        }

        // Salvează sau actualizează FAQ
        private async Task SaveOrUpdateFAQ(int productId, string question, string answer)
        {
            // Verifică dacă întrebarea există deja (aproximativ)
            var existingFAQ = await _context.ProductFAQs
                .FirstOrDefaultAsync(f => f.ProductId == productId &&
                                         EF.Functions.Like(f.Question, $"%{question.Substring(0, Math.Min(30, question.Length))}%"));

            if (existingFAQ != null)
            {
                existingFAQ.TimesAsked++;
                if (string.IsNullOrEmpty(existingFAQ.Answer))
                {
                    existingFAQ.Answer = answer;
                }
            }
            else
            {
                var faq = new ProductFAQ
                {
                    ProductId = productId,
                    Question = question.Length > 500 ? question.Substring(0, 500) : question,
                    Answer = answer,
                    TimesAsked = 1,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ProductFAQs.Add(faq);
            }

            await _context.SaveChangesAsync();
        }

        // Helper: verifică dacă textul conține oricare din cuvintele date
        private bool ContainsAny(string text, params string[] words)
        {
            return words.Any(word => text.Contains(word));
        }
    }

    // ===== REQUEST/RESPONSE MODELS =====

    public class AIQuestionRequest
    {
        public int ProductId { get; set; }
        public string Question { get; set; } = null!;
    }

    public class AIAnswerResponse
    {
        public int ProductId { get; set; }
        public string Question { get; set; } = null!;
        public string Answer { get; set; } = null!;
    }
}