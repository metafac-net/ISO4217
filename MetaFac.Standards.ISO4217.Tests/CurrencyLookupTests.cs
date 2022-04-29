using FluentAssertions;
using MetaFac.Standards.ISO4217;
using Xunit;

namespace MetaFac.Standards.ISO4217.Tests
{
    public class CurrencyLookupTests
    {

        [Theory]
        [InlineData("EUR", 35)]
        [InlineData("UAH", 1)]
        [InlineData("USD", 19)]
        [InlineData("usd", 19)]
        [InlineData("", 3)]
        [InlineData(null, 0)]
        [InlineData("ZZZ", 0)]
        [InlineData("random string", 0)]
        public void GetAllByCode(string code, int expectedCount)
        {
            var currencies = Iso4217Helper.GetAllByCode(code);

            currencies.Should().HaveCount(expectedCount);
        }

        [Theory]
        [InlineData("USD", "US Dollar", 2)]
        [InlineData("EUR", "Euro", 2)]
        [InlineData("GBP", "Pound Sterling", 2)]
        [InlineData("JPY", "Yen", 0)]
        [InlineData("CNY", "Yuan Renminbi", 2)]
        [InlineData("AUD", "Australian Dollar", 2)]
        [InlineData("CAD", "Canadian Dollar", 2)]
        [InlineData("NZD", "New Zealand Dollar", 2)]
        [InlineData("XOF", "CFA Franc BCEAO", 0)]
        [InlineData("BHD", "Bahraini Dinar", 3)]
        [InlineData("CLF", "Unidad de Fomento", 4)]
        [InlineData("XAU", "Gold", null)]
        [InlineData("XDR", "SDR (Special Drawing Right)", null)]
        public void GetByCode(string code, string? expectedFullName, int? expectedDecimals)
        {
            var info = Iso4217Helper.GetByCode(code);

            info.Should().NotBeNull();
            info?.FullName.Should().Be(expectedFullName);
            info?.Decimals.Should().Be(expectedDecimals);
        }

        [Theory]
        [InlineData("AAA")]
        [InlineData("ZZZ")]
        [InlineData(null)]
        public void GetByCodeFails(string code)
        {
            var info = Iso4217Helper.GetByCode(code);

            info.Should().BeNull();
        }

        [Theory]
        [InlineData(978, "EUR")]
        [InlineData(980, "UAH")]
        [InlineData(840, "USD")]
        [InlineData(000, "")]
        public void GetByNum(int num, string? expectedCode)
        {
            var info = Iso4217Helper.GetByNum(num);
            info?.Code.Should().Be(expectedCode);
        }

        [Theory]
        [InlineData(980, 1)]
        [InlineData(978, 35)]
        [InlineData(840, 19)]
        [InlineData(000, 3)]
        [InlineData(999, 1)]
        public void GetAllByNum(int num, int expectedCount)
        {
            var currencies = Iso4217Helper.GetAllByNum(num);
            currencies.Should().HaveCount(expectedCount);
        }

        [Theory]
        [InlineData(123)]
        public void GetAllByNumFails(int num)
        {
            var currencies = Iso4217Helper.GetAllByNum(num);
            currencies.Should().HaveCount(0);
        }
    }
}