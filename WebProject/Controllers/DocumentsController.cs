using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Services;

namespace WebProject.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly DocumentService _documentService;
        private readonly LogService _logService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentsController(
            DocumentService documentService,
            LogService logService,
            UserManager<ApplicationUser> userManager)
        {
            _documentService = documentService;
            _logService = logService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Receipt(int orderId)
        {
            var pdf = await _documentService.GenerateReceiptPdfAsync(orderId);

            await _logService.AddLogAsync(
                "Catering Receipt Generated",
                $"Receipt PDF generated for catering order #{orderId}.",
                _userManager.GetUserId(User));

            return File(pdf, "application/pdf", $"catering_receipt_order_{orderId}.pdf");
        }

        public async Task<IActionResult> Agreement(int orderId)
        {
            var pdf = await _documentService.GenerateAgreementPdfAsync(orderId);

            await _logService.AddLogAsync(
                "Catering Agreement Generated",
                $"Agreement PDF generated for catering order #{orderId}.",
                _userManager.GetUserId(User));

            return File(pdf, "application/pdf", $"catering_agreement_order_{orderId}.pdf");
        }
    }
}