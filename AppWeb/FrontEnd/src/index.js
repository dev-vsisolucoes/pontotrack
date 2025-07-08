import React from "react";
import ReactDOM from "react-dom";

import App from "./app";

let baseName = "/clientes/pontotrack";
let service = "https://www.vsisolucoes.com.br/clientes/pontotrack/api/";
let pathRpt = "/clientes/pontotrack";

if (window.location.href.indexOf("localhost") > 0) {
  baseName = "";
  service = "http://localhost/pontotrack/api/";
  pathRpt = "http://localhost/pontotrack";
}

const config = {
  baseName: baseName,
  service: service,
  pathRpt: pathRpt
};

document.title = "pontotrack";

ReactDOM.render(<App config={config} />, document.getElementById("root"));

    