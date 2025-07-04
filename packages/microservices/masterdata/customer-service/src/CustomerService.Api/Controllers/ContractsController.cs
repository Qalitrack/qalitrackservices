using Microsoft.AspNetCore.Mvc;
using CustomerService.Core.DTOs;
using CustomerService.Core.Interfaces;

namespace CustomerService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : BaseController
{
    private readonly ICustomerService _customerService;

    public ContractsController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContract(string id, [FromBody] UpdateCustomerContractRequest request)
    {
        try
        {
            var contract = await _customerService.UpdateContractAsync(id, request);
            return HandleResult(Success(contract, "Contract updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(Error<CustomerContractDto>(ex.Message));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<CustomerContractDto>(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContract(string id)
    {
        try
        {
            var result = await _customerService.DeleteContractAsync(id);
            if (!result)
            {
                return NotFound(Error($"Contract with ID {id} not found"));
            }

            return HandleResult(Success("Contract deleted successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error(ex.Message));
        }
    }
}