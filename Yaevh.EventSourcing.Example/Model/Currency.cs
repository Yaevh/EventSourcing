namespace Yaevh.EventSourcing.Example.Model;

public record Currency
{
    public string CurrencyCode { get; }
    public Currency(string currencyCode) {  CurrencyCode = currencyCode; }
}
