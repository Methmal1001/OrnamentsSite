using LankaOrnaments.Core.Constants;
using LankaOrnaments.Core.Contracts;
using LankaOrnaments.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LankaOrnaments.API.Controllers;

[ApiController]
[Route("api/ornaments")]
public sealed class OrnamentsController(IOrnamentService ornamentService) : ControllerBase
{
    /// <summary>Lists ornaments. Optional: <c>search</c>, <c>category</c>, <c>availability</c>, <c>sort</c>.</summary>
    [HttpGet]
    [AllowAnonymous]
    public Task<IReadOnlyList<OrnamentDto>> List([FromQuery] OrnamentQuery query, CancellationToken cancellationToken) =>
        ornamentService.ListAsync(query, cancellationToken);

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<OrnamentDto> Get(int id, CancellationToken cancellationToken) =>
        ornamentService.GetAsync(id, cancellationToken);

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<OrnamentDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(OrnamentInput input, CancellationToken cancellationToken)
    {
        var created = await ornamentService.CreateAsync(input, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = int.Parse(created.Id, System.Globalization.CultureInfo.InvariantCulture) }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public Task<OrnamentDto> Update(int id, OrnamentInput input, CancellationToken cancellationToken) =>
        ornamentService.UpdateAsync(id, input, cancellationToken);

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await ornamentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
