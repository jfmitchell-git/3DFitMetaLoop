//This is default template file for PlayFab CloudScript.
//Environement URLs must be updated and file saved in PlayFab's CloudScript Revision system. 

var defaultEnvironement = "dev";
var productionUrl = "https://metaloopdemo.azurewebsites.net/api/";
var stagingUrl = "https://metaloopdemo.azurewebsites.net/api/";
//Development URL must be accessible from outside (PlayFab)
var devUrl = "http://mitchell-pc.dynamic-dns.net:45455/api/";

function InvokeBackOffice(args, context) {

    var url = "";
    var isStack = isIterable(args);
    var firtsRequest = isStack ? args[0] : args;

    if (firtsRequest && firtsRequest.Method) {

        if (firtsRequest.Environement.toLowerCase() == "prod") {

            url = productionUrl + firtsRequest.Method;
        } else if (firtsRequest.Environement.toLowerCase() == "staging") {

            url = stagingUrl + firtsRequest.Method;
        } else if (firtsRequest.Environement.toLowerCase() == "dev") {

            url = devUrl + firtsRequest.Method;
        } else {

            url = productionUrl + firtsRequest.Method;
        }
    }

    if (isStack) url += "/stack";

    var playerIdParam;

    if (typeof currentPlayerId == 'undefined') {
        playerIdParam = "";
    } else {
        playerIdParam = currentPlayerId;
    }

    var body = {
        CloudScriptMethod: args,
        UserId: playerIdParam,
    };

    var headers;
    var content = JSON.stringify(body);
    var httpMethod = "post";
    var contentType = "application/json";


    // Try Catch Retry Logic important with PlayFab, should be better written with short delay
    try {
        var result = http.request(url, httpMethod, content, contentType, headers);
        return result;
    } catch (e) {
        var msg = JSON.stringify(e);

        server.WritePlayerEvent({
            PlayFabId: currentPlayerId,
            Body: { errorMessage: msg },
            EventName: "cloudscript_http_error"
        });
    }

    sleep(50);

    var result2 = http.request(url, httpMethod, content, contentType, headers);
    return { result2 };

};

function isIterable(obj) {
    if (obj == null) {
        return false;
    }
    return typeof obj[Symbol.iterator] === 'function';
}

function sleep(milliseconds) {
    const date = Date.now();
    let currentDate = null;
    do {
        currentDate = Date.now();
    } while (currentDate - date < milliseconds);
}

handlers.addInboxMessage = function (args, context) {

    var CloudScriptMethod = {
        Environement: defaultEnvironement,
        Method: "AddInboxMessage",
        Params: {
            "Content": JSON.stringify(args)
        }
    };

    var response = InvokeBackOffice(CloudScriptMethod, context);
    return response;
};

handlers.invokeBioIncBackOffice = function (args, context) {
    return InvokeBackOffice(args, context);
};



