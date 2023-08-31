using System.ComponentModel.DataAnnotations.Schema;

namespace NetBank.Domain.Dto;

[NotMapped]
public class RangeNumber
{
    public int MinValue { get; set; }
    public int MaxValue { get; set; }
}