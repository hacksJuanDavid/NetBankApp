using NetBank.Application.Interfaces;
using NetBank.Domain.Define;
using NetBank.Domain.Dto;
using NetBank.Domain.Models;
using NetBank.Domain.Interfaces.Repositories;
using NetBank.Utilities;

namespace NetBank.Application.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly IIssuingNetworkRepository _issuingNetworkRepository;
        private const string NUMBER_REGEX = "^[0-9]*$";

        public CreditCardService(IIssuingNetworkRepository issuingNetworkRepository)
        {
            _issuingNetworkRepository = issuingNetworkRepository;
        }

        public async Task<ValidationResultType> Validate(string creditCardNumber)
        {   
            bool isValid = CreditCardValidator.IsValid(creditCardNumber);

            if (!isValid)
            {
                return ValidationResultType.BadRequest;
            }

            List<IssuingNetwork> issuingNetworks = await GetIssuingNetworks();

            if (issuingNetworks.Count == 0)
            {
                return ValidationResultType.NotFound;
            }

            List<IssuingNetworkData> issuingNetworkDataList = await LoadIssuingNetworkData();

            IssuingNetworkData issuingNetworkData = issuingNetworkDataList.FirstOrDefault(x => x.Name == issuingNetworks[0].Name);

            if (issuingNetworkData == null)
            {
                return ValidationResultType.NotFound;
            }

            await GetIssuingNetworkData(issuingNetworkData.Id);

            Result = new CreditCardResult(issuingNetworkData.Name, true);

            return ValidationResultType.Ok;
        }

        public CreditCardResult Result { get; set; }

        private async Task<List<IssuingNetworkData>> LoadIssuingNetworkData()
        {
            // Replace this with your actual data retrieval logic
            // For now, let's assume some sample data
            List<IssuingNetworkData> issuingNetworkDataList = new List<IssuingNetworkData>
            {
                new IssuingNetworkData
                {
                    Name = "Visa",
                    StartsWithNumbers = new List<int> { 4 },
                    InRange = new RangeNumber { MinValue = 13, MaxValue = 19 },
                    AllowedLengths = new List<int> { 16 }
                },
                // Add more IssuingNetworkData instances as needed
            };

            return issuingNetworkDataList;
        }

        private async Task<List<IssuingNetwork>> GetIssuingNetworks()
        {
            // Replace this with your actual data retrieval logic
            // For now, let's assume some sample data
            List<IssuingNetwork> issuingNetworks = new List<IssuingNetwork>
            {
                new IssuingNetwork { Name = "Visa" },
                // Add more IssuingNetwork instances as needed
            };

            return issuingNetworks;
        }

        private async Task GetIssuingNetworkData(int id)
        {   
            await Task.Yield();
            // Replace this with your actual data retrieval logic
            // For now, let's assume some sample data
            IssuingNetworkData issuingNetworkData = new IssuingNetworkData
            {
                Name = "Visa",
                StartsWithNumbers = new List<int> { 4 },
                InRange = new RangeNumber { MinValue = 13, MaxValue = 19 },
                AllowedLengths = new List<int> { 16 }
            };

            // Use the retrieved data as needed
        }

        Task<ValidationResultType> ICreditCardService.Validate(string creditCardNumber)
        {
            return Validate(creditCardNumber);
        }
    }
}
