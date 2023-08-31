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
            Console.WriteLine($"Validating credit card number: {creditCardNumber}");

            bool isValid = CreditCardValidator.IsValid(creditCardNumber);

            if (!isValid)
            {
                Console.WriteLine("Invalid credit card number");
                Result = new CreditCardResult("BadRequest", isValid);
                return ValidationResultType.BadRequest;
            }

            List<IssuingNetworkData> issuingNetworkDataList = await LoadIssuingNetworkData();

            foreach (var network in issuingNetworkDataList)
            {
                Console.WriteLine($"IsInRange: {network.InRange?.MinValue} - {network.InRange?.MaxValue}");
                if (network.StartsWithNumbers?.Any(number => creditCardNumber.StartsWith(number.ToString())) == true ||
                    (network.InRange != null && CreditCardValidator.IsInRange(creditCardNumber, network.InRange.MinValue, network.InRange.MaxValue)))
                {   
                    Console.WriteLine($"Match found for {network.Name}");
                    await GetIssuingNetworkData(network.Id);
                    Result = new CreditCardResult(network.Name, isValid);
                    return ValidationResultType.Ok;
                }
            }

            Console.WriteLine("No match found for any network");
            Result = new CreditCardResult("NotFound", isValid);
            return ValidationResultType.NotFound;
        }


        private async Task<List<IssuingNetwork>> GetIssuingNetworks()
        {
            IEnumerable<IssuingNetwork> issuingNetworks = await _issuingNetworkRepository.GetAllAsync();

            return (List<IssuingNetwork>)issuingNetworks;
        }

        private async Task GetIssuingNetworkData(int id)
        {
            IssuingNetwork issuingNetwork = await _issuingNetworkRepository.GetByIdAsync(id);

            Result = new CreditCardResult(issuingNetwork.Name, true);
        }

        public CreditCardResult Result { get; set; }

        private async Task<List<IssuingNetworkData>> LoadIssuingNetworkData()
        {
            IEnumerable<IssuingNetwork> issuingNetworks = await _issuingNetworkRepository.GetAllAsync();
            
            return issuingNetworks.Select(network => new IssuingNetworkData
            {
                Id = network.Id,
                Name = network.Name,
                StartsWithNumbers = network.StartsWithNumbers?.Split(',').Select(int.Parse).ToList(),
                AllowedLengths = network.AllowedLengths.Split(',').Select(int.Parse).ToList(),
                InRange = network.InRange
            }).ToList();
        }

        Task<ValidationResultType> ICreditCardService.Validate(string creditCardNumber)
        {
            return Validate(creditCardNumber);
        }
    }
}