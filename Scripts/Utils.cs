// Static class with some constants used in the game
using System;

public static class Utils
{
  public static float ViewportBaseX = 800;
  public static float ViewportBaseY = 600;
  public const int BoardSize = 8;
  public const int CheckerSize = 48;
  public const int TileSize = 48;

  public enum Currency
  {
    USD,
    EUR,
    GBP,
    BRL,
    JPY,
    INR,
    ILS,
    SEK,
    NOK,
    ISK,
    THB,
    VND
  }

  public enum SymbolPosition
  {
    Left,
    Right
  }

  public static (string Symbol, SymbolPosition Position) GetCurrencySymbol(Currency currency)
  {
    switch (currency)
    {
      case Currency.USD:
        return ("$", SymbolPosition.Right);
      case Currency.EUR:
        return ("€", SymbolPosition.Right);
      case Currency.GBP:
        return ("£", SymbolPosition.Right);
      case Currency.BRL:
        return ("R$", SymbolPosition.Left);
      case Currency.JPY:
        return ("¥", SymbolPosition.Left);
      case Currency.INR:
        return ("₹", SymbolPosition.Left);
      case Currency.ILS:
        return ("₪", SymbolPosition.Left);
      case Currency.SEK:
        return ("kr", SymbolPosition.Right);
      case Currency.NOK:
        return ("kr", SymbolPosition.Right);
      case Currency.ISK:
        return ("kr", SymbolPosition.Right);
      case Currency.THB:
        return ("฿", SymbolPosition.Left);
      case Currency.VND:
        return ("₫", SymbolPosition.Right);
      default:
        throw new ArgumentOutOfRangeException(nameof(currency), currency, "Unsupported currency.");
    }
  }

  public static string FormatMoneyText(string money, Currency currency)
  {
      var currencyInfo = GetCurrencySymbol(currency);
      if (currencyInfo.Position == SymbolPosition.Left)
        return $"{currencyInfo.Symbol}{money}";
      else
        return $"{money}{currencyInfo.Symbol}";
  }
}
