using Microsoft.AspNetCore.Mvc;
using SaccoService.Core.DTOs;
using SaccoService.Core.Interfaces;

namespace SaccoService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaccosController : ControllerBase
{
    private readonly ISaccoService _saccoService;

    public SaccosController(ISaccoService saccoService)
    {
        _saccoService = saccoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaccoDto>>> GetSaccos()
    {
        var saccos = await _saccoService.GetSaccosAsync();
        return Ok(saccos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SaccoDto>> GetSacco(string id)
    {
        var sacco = await _saccoService.GetSaccoAsync(id);
        if (sacco == null)
        {
            return NotFound();
        }
        return Ok(sacco);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<SaccoDetailDto>> GetSaccoDetails(string id)
    {
        var sacco = await _saccoService.GetSaccoDetailsAsync(id);
        if (sacco == null)
        {
            return NotFound();
        }
        return Ok(sacco);
    }

    [HttpPost]
    public async Task<ActionResult<SaccoDto>> RegisterSacco(RegisterSaccoRequest request)
    {
        try
        {
            var sacco = await _saccoService.RegisterSaccoAsync(request);
            return CreatedAtAction(nameof(GetSacco), new { id = sacco.Id }, sacco);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SaccoDto>> UpdateSacco(string id, UpdateSaccoRequest request)
    {
        try
        {
            var sacco = await _saccoService.UpdateSaccoAsync(id, request);
            return Ok(sacco);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<SaccoMemberDto>>> GetSaccoMembers(string id)
    {
        var members = await _saccoService.GetSaccoMembersAsync(id);
        return Ok(members);
    }

    [HttpPost("{id}/members")]
    public async Task<ActionResult<SaccoMemberDto>> AddMember(string id, AddMemberRequest request)
    {
        try
        {
            var member = await _saccoService.AddMemberAsync(id, request);
            return Ok(member);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/committee")]
    public async Task<ActionResult<IEnumerable<SaccoCommitteeDto>>> GetCommitteeMembers(string id)
    {
        var committee = await _saccoService.GetCommitteeMembersAsync(id);
        return Ok(committee);
    }

    [HttpPost("{id}/committee")]
    public async Task<ActionResult<SaccoCommitteeDto>> AddCommitteeMember(string id, AddCommitteeMemberRequest request)
    {
        try
        {
            var committeeMember = await _saccoService.AddCommitteeMemberAsync(id, request);
            return Ok(committeeMember);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/meetings")]
    public async Task<ActionResult<IEnumerable<SaccoMeetingDto>>> GetMeetings(string id)
    {
        var meetings = await _saccoService.GetMeetingsAsync(id);
        return Ok(meetings);
    }

    [HttpPost("{id}/meetings")]
    public async Task<ActionResult<SaccoMeetingDto>> ScheduleMeeting(string id, ScheduleMeetingRequest request)
    {
        try
        {
            var meeting = await _saccoService.ScheduleMeetingAsync(id, request);
            return Ok(meeting);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id}/financials")]
    public async Task<ActionResult<SaccoFinancialDto>> GetFinancials(string id)
    {
        var financials = await _saccoService.GetFinancialInformationAsync(id);
        if (financials == null)
        {
            return NotFound();
        }
        return Ok(financials);
    }

    [HttpGet("{id}/shares")]
    public async Task<ActionResult<IEnumerable<SaccoShareDto>>> GetShares(string id)
    {
        var shares = await _saccoService.GetSharesAsync(id);
        return Ok(shares);
    }

    // Regulatory Compliance Endpoints - Commented out due to missing DTOs
    /*
    [HttpGet("{id}/compliance-status")]
    public async Task<ActionResult<SaccoComplianceStatusDto>> GetComplianceStatus(string id)
    {
        try
        {
            var status = await _saccoService.GetComplianceStatusAsync(id);
            return Ok(status);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id}/compliance-check")]
    public async Task<ActionResult<SaccoComplianceReportDto>> PerformComplianceCheck(string id)
    {
        try
        {
            var report = await _saccoService.PerformComplianceCheckAsync(id);
            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id}/regulatory-submission")]
    public async Task<ActionResult<SaccoRegulatorySubmissionDto>> SubmitRegulatoryReport(
        string id, [FromBody] SubmitRegulatoryReportRequest request)
    {
        try
        {
            var submission = await _saccoService.SubmitRegulatoryReportAsync(id, request);
            return Ok(submission);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/regulatory-submissions")]
    public async Task<ActionResult<IEnumerable<SaccoRegulatorySubmissionDto>>> GetRegulatorySubmissions(string id)
    {
        try
        {
            var submissions = await _saccoService.GetRegulatorySubmissionsAsync(id);
            return Ok(submissions);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("compliance-violations")]
    public async Task<ActionResult<IEnumerable<SaccoComplianceViolationDto>>> GetComplianceViolations()
    {
        var violations = await _saccoService.GetComplianceViolationsAsync();
        return Ok(violations);
    }

    [HttpPost("{id}/external-audit")]
    public async Task<ActionResult<SaccoAuditResultDto>> ScheduleExternalAudit(
        string id, [FromBody] ScheduleAuditRequest request)
    {
        try
        {
            var audit = await _saccoService.ScheduleExternalAuditAsync(id, request);
            return Ok(audit);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }
    */
}