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
        //private const string NUMBER_REGEX = "^[0-9]*$";

        public CreditCardService(IIssuingNetworkRepository issuingNetworkRepository)
        {
            _issuingNetworkRepository = issuingNetworkRepository;
        }
        
        public CreditCardResult Result { get;  set; } = null!;
        
        public async Task<ValidationResultType> Validate(string creditCardNumber)
        {
            try
            {
                Console.WriteLine($"Validating credit card number: {creditCardNumber}");

                bool isValid = CreditCardValidator.IsValid(creditCardNumber);

                if (!isValid)
                {
                    Console.WriteLine("Invalid credit card number");
                    Result = new CreditCardResult("Bad Request", isValid);
                    return ValidationResultType.BadRequest;
                }
                
                List<IssuingNetworkData> issuingNetworkDataList = await LoadIssuingNetworkData();

                foreach (var network in issuingNetworkDataList)
                {   
                    if (network.StartsWithNumbers?.Any(number => creditCardNumber.StartsWith(number.ToString())) == true ||
                        (network.InRange != null && IsInRange(creditCardNumber, network.InRange)))
                    {
                        Console.WriteLine($"Match found for {network.Name}");
                        await GetIssuingNetworkData(network.Id);
                        Result = new CreditCardResult(network.Name, isValid);
                        return ValidationResultType.Ok;
                    }
            
                    if (network.StartsWithNumbers?.Any(number => creditCardNumber.StartsWith(number.ToString())) == true)
                    {
                        Console.WriteLine($"StartsWithNumbers match found for {network.Name}");
                    }

                    if (network.InRange != null && IsInRange(creditCardNumber, network.InRange))
                    {
                        Console.WriteLine($"InRange match found for {network.Name}");
                    }
                }

                Console.WriteLine("No match found for any network");
                Result = new CreditCardResult("Not Found", isValid);
                return ValidationResultType.NotFound;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during credit card validation: {ex.Message}");
                throw;
            }
        }


        private async Task GetIssuingNetworkData(int id)
        {
            IssuingNetwork issuingNetwork = await _issuingNetworkRepository.GetByIdAsync(id);

            Result = new CreditCardResult(issuingNetwork.Name, true);
        }

        private async Task<List<IssuingNetworkData>> LoadIssuingNetworkData()
        {
            IEnumerable<IssuingNetwork> issuingNetworks = await _issuingNetworkRepository.GetAllAsync();

            return issuingNetworks.Select(network => new IssuingNetworkData
            {
                Id = network.Id,
                Name = network.Name,
                StartsWithNumbers = network.StartsWithNumbers?.Split(',').Select(int.Parse).ToList(),
                AllowedLengths = network.AllowedLengths.Split(',').Select(int.Parse).ToList(),
                InRange = ParseRange(network.InRange)
            }).ToList();
        }
        
        private bool IsInRange(string creditCardNumber, RangeNumber range)
        {
            int numberLength = range.MinValue.ToString().Length;
            
            int initialDigists = int.Parse(creditCardNumber.Substring(0, numberLength));
          
    
            Console.WriteLine("\n");
            Console.WriteLine("Function IsInRange");
            Console.WriteLine($"CardNumber: {initialDigists}, MinValue: {range.MinValue}, MaxValue: {range.MaxValue}");

            var result = initialDigists >= range.MinValue && initialDigists <= range.MaxValue;
            Console.WriteLine($"IsInRange: {result}");
            return result;
        }

        
        private RangeNumber? ParseRange(string? rangeString)
        {
            if (string.IsNullOrEmpty(rangeString)) return null;

            var range = rangeString.Split('-');
            if (range.Length != 2)
            {
                Console.WriteLine("Invalid range format");
                return null;
            }
    
            if (!int.TryParse(range[0], out int minValue) || !int.TryParse(range[1], out int maxValue))
            {
                Console.WriteLine("Invalid range values");
                return null;
            }
    
            Console.WriteLine("\n");
            Console.WriteLine("Function ParseRange");
            Console.WriteLine($"MinValue: {minValue}, MaxValue: {maxValue}");

            return new RangeNumber(minValue, maxValue);
        }

        Task<ValidationResultType> ICreditCardService.Validate(string creditCardNumber)
        {
            return Validate(creditCardNumber);
        }
    }
}