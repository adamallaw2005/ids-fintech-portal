using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/documents")]
public sealed class DocumentsController(IDocumentService documentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentDetails>>> GetAll(
        [FromQuery] int? productId,
        [FromQuery] string? documentType)
    {
        return Ok(await documentService.GetAllAsync(productId, documentType));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentDetails>> GetById(int id)
    {
        var document = await documentService.GetByIdAsync(id);
        return document is null ? NotFound() : Ok(document);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<DocumentDetails>> Create(DocumentRequest request)
    {
        var documentId = await documentService.CreateAsync(request);
        var document = await documentService.GetByIdAsync(documentId);
        return CreatedAtAction(nameof(GetById), new { id = documentId }, document);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DocumentRequest request)
    {
        return await documentService.UpdateAsync(id, request) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await documentService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
