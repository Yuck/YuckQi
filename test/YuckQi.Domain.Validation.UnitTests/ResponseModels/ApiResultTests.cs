using NUnit.Framework;
using System.Linq;
using System.Text.Json;
using YuckQi.Domain.Validation.JsonConverters;
using YuckQi.Domain.Validation.ResponseModels;

namespace YuckQi.Domain.Validation.UnitTests.ResponseModels
{
    public class ApiResultTests
    {
        [Test]
        public void Result_CanBeDeserialized_ToApiResult()
        {
            var message = "a test message";
            var code = "a test code";
            var property = "property";
            var type = ResultType.Warning;
            var detail = new ResultDetail(message, code, property, type);
            var content = "this is some test content";
            var result = new Result<string>(content, [detail]);
            var options = new JsonSerializerOptions { Converters = { new ResultCodeJsonConverter(), new ResultMessageJsonConverter() } };
            var json = JsonSerializer.Serialize(result, options);
            var response = JsonSerializer.Deserialize<ApiResult<string>>(json, options);

            Assert.That(response, Is.Not.Null);
            Assert.That(response!.Content, Is.Not.Null);
            Assert.That(response!.Content, Is.EqualTo(content));
            Assert.That(response!.Detail, Is.Not.Null);
            Assert.That(response!.Detail, Is.Not.Empty);
            Assert.That(response!.Detail, Has.Count.EqualTo(1));
            Assert.That(response!.Detail, Has.Exactly(1).Matches<ApiResultDetail>(t => t.Message == message));
            Assert.That(response!.Detail, Has.Exactly(1).Matches<ApiResultDetail>(t => t.Code == code));
            Assert.That(response!.Detail, Has.Exactly(1).Matches<ApiResultDetail>(t => t.Property == property));
            Assert.That(response!.Detail, Has.Exactly(1).Matches<ApiResultDetail>(t => t.Type == type));
        }

        [Test]
        public void ApiResult_NewInstance_HasEmptyDetail()
        {
            var result = new ApiResult();

            Assert.That(result.Detail, Is.Not.Null);
            Assert.That(result.Detail, Is.Empty);
        }

        [Test]
        public void ApiResult_Generic_Content_HasMatchingValue()
        {
            var content = 42;
            var result = new ApiResult<int> { Content = content };

            Assert.That(result.Content, Is.EqualTo(content));
        }

        [Test]
        public void ApiResult_Generic_DefaultContent_IsDefault()
        {
            var result = new ApiResult<string>();

            Assert.That(result.Content, Is.Null);
        }

        [Test]
        public void ApiResult_Equality_IsBasedOnDetail()
        {
            var a = new ApiResult();
            var b = new ApiResult();

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void ApiResult_Generic_Equality_IsBasedOnContent()
        {
            var a = new ApiResult<int> { Content = 1 };
            var b = new ApiResult<int> { Content = 1 };

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void ApiResult_WithExpression_CopiesDetail()
        {
            var original = new ApiResult();
            var copy = original with { };

            Assert.That(copy.Detail, Is.EqualTo(original.Detail));
        }

        [Test]
        public void ApiResult_Generic_WithExpression_CopiesContent()
        {
            var original = new ApiResult<int> { Content = 42 };
            var copy = original with { Content = 99 };

            Assert.That(copy.Content, Is.EqualTo(99));
        }

        [Test]
        public void ApiResult_ToString_IsNotEmpty()
        {
            var result = new ApiResult();

            Assert.That(result.ToString(), Is.Not.Empty);
        }

        [Test]
        public void ApiResult_Generic_ToString_IsNotEmpty()
        {
            var result = new ApiResult<int> { Content = 5 };

            Assert.That(result.ToString(), Is.Not.Empty);
        }
    }
}
