using System.Collections.Generic;
using System.Threading.Tasks;
using Kotova.Test1.ClientSide.ManagementWPF.Models;

namespace Kotova.Test1.ClientSide.ManagementWPF.Services
{
    public interface IApiService
    {
        Task<List<DepartmentWithChiefsDto>> GetDepartmentsWithChiefsAsync();
        Task<string> AssignUnplannedInstructionToChiefsAsync(UnplannedInstructionForChiefsPackage package);
        Task<List<UnplannedInstructionStatusDto>> GetUnplannedInstructionsStatusAsync();
        Task<List<NormativeInstructionDto>> GetNormativeInstructionsAsync();
    }

    public class NormativeInstructionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }
}