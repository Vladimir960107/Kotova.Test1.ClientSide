using System.Collections.Generic;
using System.Threading.Tasks;
using Kotova.Test1.ClientSide.ManagementWPF.Models;
using Kotova.CommonClasses; // Add this using for the common DTOs

namespace Kotova.Test1.ClientSide.ManagementWPF.Services
{
    public interface IApiService
    {
        Task<List<DepartmentWithChiefsDto>> GetDepartmentsWithChiefsAsync();
        Task<string> AssignUnplannedInstructionToChiefsAsync(UnplannedInstructionForChiefsPackage package);
        Task<string> AssignUnplannedInstructionToDepartmentsAsync(UnplannedInstructionForDepartmentsPackage package);
        Task<List<UnplannedInstructionStatusDto>> GetUnplannedInstructionsStatusAsync();

        // Update these methods to use the new DTOs from CommonClasses:
        Task<List<NormativeInstructionDto>> GetNormativeInstructionsAsync(bool? isUnplannedInstruction = null);
        Task<NormativeInstructionDto> CreateNormativeInstructionAsync(NormativeInstructionCreateDto model);
        Task<NormativeInstructionDto> UpdateNormativeInstructionAsync(int id, NormativeInstructionUpdateDto model);
        Task<bool> DeleteNormativeInstructionAsync(int id);

        void SetAuthToken(string jwtToken);
    }
}