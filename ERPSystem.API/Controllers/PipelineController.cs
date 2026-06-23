using ERPSystem.Application.DTOs.CRM;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ERPSystem.API.Controllers.CRM
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PipelineController : ControllerBase
    {
        private readonly IPipelineService _pipelineService;
        private readonly ICurrentUserService _currentUserService;

        public PipelineController(IPipelineService pipelineService, ICurrentUserService currentUserService)
        {
            _pipelineService = pipelineService;
            _currentUserService = currentUserService;
        }


        [HttpGet]
        [ProducesResponseType(typeof(List<PipelineDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<PipelineDto>>> GetAll(CancellationToken ct = default)
        {
            try
            {
                var companyId = _currentUserService.CompanyId;
                var pipelines = await _pipelineService.ListAsync(companyId, ct);
                return Ok(pipelines);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PipelineDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<PipelineDto>> GetById(int id, CancellationToken ct = default)
        {
            try
            {
                var companyId = _currentUserService.CompanyId;
                var pipeline = await _pipelineService.GetByIdAsync(id, companyId, ct);

                if (pipeline == null)
                    return NotFound(new { message = "Pipeline not found." });

                return Ok(pipeline);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

       

        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> Create([FromBody] CreatePipelineDto dto, CancellationToken ct = default)
        {
            try
            {
                var companyId = _currentUserService.CompanyId;
                var pipelineId = await _pipelineService.CreateAsync(dto, companyId, ct);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = pipelineId },
                    new { id = pipelineId, message = "Pipeline created successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Update(int id, [FromBody] UpdatePipelineDto dto, CancellationToken ct = default)
        {
            try
            {
                var companyId = _currentUserService.CompanyId;
                await _pipelineService.UpdateAsync(id, dto, companyId, ct);

                return Ok(new { message = "Pipeline updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                var companyId = _currentUserService.CompanyId;
                await _pipelineService.DeleteAsync(id, companyId, ct);

                return Ok(new { message = "Pipeline deleted successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/move-stage")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> MoveStage(int id, [FromBody] MovePiplineStageDto dto, CancellationToken ct = default)
        {
            try
            {
                var companyId = _currentUserService.CompanyId;
                await _pipelineService.MoveStageAsync(id, dto, companyId, ct);

                return Ok(new { message = "Pipeline stage updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
