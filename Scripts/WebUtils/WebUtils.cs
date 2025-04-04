using Godot;
using System.Collections.Generic;


public class UrlParamsModel
{
	public string Token { get; set; }
	public string SessionId { get; set; }
	public string Currency { get; set; }
	public string Prefix { get; set; }

	public UrlParamsModel(string token, string sessionId, string currency, string prefix)
	{
		Token = token;
		SessionId = sessionId;
		Currency = currency;
		Prefix = prefix;
	}
}

// Here are some examples of URL params to add to our game url.

// ?token=abc123&sessionid=xyz456&currency=USD
// ?token=token456&sessionid=session2&currency=USD
public static class WebUtils
{
	public static Dictionary<string, string> GetUrlParams()
	{
		string osName = OS.GetName();

		if (osName != "Web" && osName != "HTML5")
		{
			GD.Print($"[ URL PARAMS ] :: Running on non-HTML environment: {OS.GetName()}. URL params will not be processed.");
			return new Dictionary<string, string>();
		}

		string script = @"
			var params = new URLSearchParams(window.location.search);
			var obj = {};
			params.forEach((value, key) => { obj[key] = value; });
			console.log('Parsed URL Params:', obj); // Debug line
			JSON.stringify(obj); // No return statement
		";

		string result = (string)JavaScript.Eval(script);

		// Debug print to see the raw JSON result
		GD.Print("[ URL PARAMS ] :: Raw JSON Result: ", result);

		var jsonParseResult = JSON.Parse(result);
		if (jsonParseResult.Error == Error.Ok && jsonParseResult.Result is Godot.Collections.Dictionary dict)
		{
			var parameters = new Dictionary<string, string>();
			foreach (var key in dict.Keys)
			{
				parameters[key.ToString()] = dict[key].ToString();
			}
			return parameters;
		}

		return new Dictionary<string, string>();
	}

	public static UrlParamsModel GetUrlParamsModel()
	{
		var parameters = GetUrlParams();
		string osName = OS.GetName();
		string token;
		string sessionId;
		string currency;
		var isWeb = osName == "Web" || osName == "HTML5";
		// Extract values safely
		if (isWeb)
		{
			token = parameters.ContainsKey("token") ? parameters["token"] : "N/A";
			sessionId = parameters.ContainsKey("sessionid") ? parameters["sessionid"] : "N/A";
			currency = parameters.ContainsKey("currency") ? parameters["currency"] : "N/A";
		}
		else
		{
			token = "b";
			sessionId = "f7c441fa-d910-4573-8537-53a14a4142d6";
			/* token = "token456";
			sessionId = "session2"; */
			currency = "BRL";
			return new UrlParamsModel(token, sessionId, currency, "ws");
		}

		// Use JavaScript to get the current protocol (http or https)
		string script = @"window.location.protocol;";

		string protocol = (string)JavaScript.Eval(script);

		// Determine the WebSocket prefix based on the protocol
		GD.Print($"[ URL PARAMS ] :: Determined WebSocket Protocol: {protocol}");
		string prefix = "wss";
		GD.Print($"[ URL PARAMS ] :: Determined WebSocket Prefix: {prefix}");

		return new UrlParamsModel(token, sessionId, currency, prefix);
	}


	public static void PrintUrlParams()
	{
		var parameters = GetUrlParams();

		// Extract values safely
		string token = parameters.ContainsKey("token") ? parameters["token"] : "N/A";
		string sessionId = parameters.ContainsKey("sessionid") ? parameters["sessionid"] : "N/A";
		string currency = parameters.ContainsKey("currency") ? parameters["currency"] : "N/A";

		// Print to Godot output
		GD.Print($"[ URL PARAMS ] :: Token: {token}, Session ID: {sessionId}, Currency: {currency}");

		// Print to browser console in a clean way
		string script = $"console.log('[ URL PARAMS ] :: Token:', '{token}', 'Session ID:', '{sessionId}', 'Currency:', '{currency}');";
		JavaScript.Eval(script);
	}

}
