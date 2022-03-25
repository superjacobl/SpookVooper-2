using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Economy.Taxes;

public enum TaxType
{
    // PersonalIncome and CorporateIncome are paid daily
    Transactional,
    Sales,
    PersonalIncome,
    CorporateIncome,
    CompanyIncome
}

public class Tax : Entity
{
    public decimal Rate { get; set;}
    public bool DistrictTax { get; set;}
    public string? District_Id { get; set;}
    public TaxType taxType { get; set;}
    // the min amount after which the tax has effect
    // example for Minimum and Maximum
    // if a sales tax has a min of $1 and a max of $3 then
    // I sell a apple for $2, then $1 will be subjected to the Rate
    // I sell a apple for $4, then $2 will be subjected to the Rate
    public decimal Minimum { get; set;}
    // the max amount after which the tax no longer has effect
    public decimal Maximum { get; set;}
    // amount this tax has collected in the current month
    public decimal Collected { get; set;}
}