using Microsoft.AspNetCore.Mvc;
using ComplianceService.Core.Entities;
using ComplianceService.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ComplianceService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplianceRulesController : ControllerBase
    {
        private readonly IComplianceRepository _repository;
        private readonly ILogger<ComplianceRulesController> _logger;

        public ComplianceRulesController(IComplianceRepository repository, ILogger<ComplianceRulesController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ComplianceRule>>> GetRules([FromQuery] string? ruleType = null, [FromQuery] bool activeOnly = true)
        {
            try
            {
                List<ComplianceRule> rules;

                if (!string.IsNullOrEmpty(ruleType))
                {
                    rules = await _repository.GetRulesByTypeAsync(ruleType);
                }
                else if (activeOnly)
                {
                    rules = await _repository.GetActiveRulesAsync();
                }
                else
                {
                    // This would require a new repository method for all rules
                    rules = await _repository.GetActiveRulesAsync(); // For now, return active only
                }

                return Ok(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance rules");
                return StatusCode(500, "An error occurred while retrieving compliance rules");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ComplianceRule>> GetRule(int id)
        {
            try
            {
                var rule = await _repository.GetRuleByIdAsync(id);
                if (rule == null)
                {
                    return NotFound($"Compliance rule with ID {id} not found");
                }

                return Ok(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance rule {RuleId}", id);
                return StatusCode(500, "An error occurred while retrieving the compliance rule");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ComplianceRule>> CreateRule([FromBody] CreateComplianceRuleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var rule = new ComplianceRule
                {
                    Name = request.Name,
                    Description = request.Description,
                    RuleType = request.RuleType,
                    Category = request.Category,
                    ConfigurationJson = request.ConfigurationJson,
                    IsActive = request.IsActive,
                    Severity = request.Severity,
                    MinValue = request.MinValue,
                    MaxValue = request.MaxValue,
                    Unit = request.Unit,
                    PenaltyAmount = request.PenaltyAmount,
                    PenaltyCurrency = request.PenaltyCurrency,
                    CreatedBy = request.CreatedBy,
                    UpdatedBy = request.CreatedBy
                };

                var createdRule = await _repository.CreateRuleAsync(rule);

                return CreatedAtAction(nameof(GetRule), new { id = createdRule.Id }, createdRule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating compliance rule");
                return StatusCode(500, "An error occurred while creating the compliance rule");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ComplianceRule>> UpdateRule(int id, [FromBody] UpdateComplianceRuleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingRule = await _repository.GetRuleByIdAsync(id);
                if (existingRule == null)
                {
                    return NotFound($"Compliance rule with ID {id} not found");
                }

                existingRule.Name = request.Name;
                existingRule.Description = request.Description;
                existingRule.RuleType = request.RuleType;
                existingRule.Category = request.Category;
                existingRule.ConfigurationJson = request.ConfigurationJson;
                existingRule.IsActive = request.IsActive;
                existingRule.Severity = request.Severity;
                existingRule.MinValue = request.MinValue;
                existingRule.MaxValue = request.MaxValue;
                existingRule.Unit = request.Unit;
                existingRule.PenaltyAmount = request.PenaltyAmount;
                existingRule.PenaltyCurrency = request.PenaltyCurrency;
                existingRule.UpdatedBy = request.UpdatedBy;

                var updatedRule = await _repository.UpdateRuleAsync(existingRule);

                return Ok(updatedRule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating compliance rule {RuleId}", id);
                return StatusCode(500, "An error occurred while updating the compliance rule");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRule(int id)
        {
            try
            {
                var existingRule = await _repository.GetRuleByIdAsync(id);
                if (existingRule == null)
                {
                    return NotFound($"Compliance rule with ID {id} not found");
                }

                await _repository.DeleteRuleAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting compliance rule {RuleId}", id);
                return StatusCode(500, "An error occurred while deleting the compliance rule");
            }
        }

        [HttpPost("{id}/activate")]
        public async Task<ActionResult> ActivateRule(int id)
        {
            try
            {
                var rule = await _repository.GetRuleByIdAsync(id);
                if (rule == null)
                {
                    return NotFound($"Compliance rule with ID {id} not found");
                }

                rule.IsActive = true;
                await _repository.UpdateRuleAsync(rule);

                return Ok(new { Message = "Rule activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating compliance rule {RuleId}", id);
                return StatusCode(500, "An error occurred while activating the compliance rule");
            }
        }

        [HttpPost("{id}/deactivate")]
        public async Task<ActionResult> DeactivateRule(int id)
        {
            try
            {
                var rule = await _repository.GetRuleByIdAsync(id);
                if (rule == null)
                {
                    return NotFound($"Compliance rule with ID {id} not found");
                }

                rule.IsActive = false;
                await _repository.UpdateRuleAsync(rule);

                return Ok(new { Message = "Rule deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating compliance rule {RuleId}", id);
                return StatusCode(500, "An error occurred while deactivating the compliance rule");
            }
        }
    }

    public class CreateComplianceRuleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ConfigurationJson { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Severity { get; set; } = "MEDIUM";
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal? PenaltyAmount { get; set; }
        public string PenaltyCurrency { get; set; } = "KES";
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class UpdateComplianceRuleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ConfigurationJson { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Severity { get; set; } = string.Empty;
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal? PenaltyAmount { get; set; }
        public string PenaltyCurrency { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}