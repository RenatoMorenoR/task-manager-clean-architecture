using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces.UseCases;

namespace TaskManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController(
    ICreateTaskUseCase createUseCase,
    IGetTasksUseCase getTasksUseCase,
    IUpdateTaskUseCase updateUseCase,
    IDeleteTaskUseCase deleteUseCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tasks = await getTasksUseCase.ExecuteAsync(ct);
        return Ok(tasks);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var task = await createUseCase.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { id = task.Id }, task);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken ct)
    {
        var task = await updateUseCase.ExecuteAsync(id, request, ct);
        return Ok(task);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await deleteUseCase.ExecuteAsync(id, ct);
        return NoContent();
    }
}
