
export default class ApiUtilities {
    static fetch(url, values, method) {

        var querystring = "XDEBUG_SESSION_START=netbeans-xdebug";

        if (method != "GET") {
            var formdata = new FormData();

            for (var prop in values) {
                formdata.append(prop, values[prop]);
            }

            formdata.append("XDEBUG_SESSION_START", "netbeans-xdebug");

        } else {

            for (var prop in values) {
                querystring += "&" + prop + "=" + values[prop];
            }
        }

        return fetch(url + "?" + querystring, {
            method: method,
            credentials: "include",
            body: formdata
        });
    }
}