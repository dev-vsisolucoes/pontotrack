<?php //

session_start();

//error_reporting(E_ERROR | E_PARSE); // | E_NOTICE | E_WARNING 

ini_set('memory_limit', '-1');

set_time_limit(0);

date_default_timezone_set('America/Sao_Paulo');

//
// Cors
//
if (isset($_SERVER['HTTP_ORIGIN'])) {
    header("Access-Control-Allow-Origin: {$_SERVER['HTTP_ORIGIN']}");
    header('Access-Control-Allow-Credentials: true');
    header('Access-Control-Max-Age: 86400');    // cache for 1 day
}

// Access-Control headers are received during OPTIONS requests
if ($_SERVER['REQUEST_METHOD'] == 'OPTIONS') {

    if (isset($_SERVER['HTTP_ACCESS_CONTROL_REQUEST_METHOD']))
        header("Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS");

    if (isset($_SERVER['HTTP_ACCESS_CONTROL_REQUEST_HEADERS']))
        header("Access-Control-Allow-Headers: {$_SERVER['HTTP_ACCESS_CONTROL_REQUEST_HEADERS']}");

    exit(0);
}

//
//
//
$host = "http://" . $_SERVER["HTTP_HOST"];

//if (isset($_SESSION["ROOT_PATH"])) {
//    if ($_SESSION["ROOT_PATH"] != $host) {
//        unset($_SESSION['APP_INITIALIZED']);
//    }
//}

$document = new DOMDocument('1.0', 'utf-8');
$document->load("config.xml");

$xpath = new DOMXPath($document);

$rootPathRemote = trim($xpath->query("//RootPath/Remote")->item(0)->nodeValue);
$rootPathLocal = trim($xpath->query("//RootPath/Local")->item(0)->nodeValue);
//$clientManagerServer = trim($xpath->query("//ClientManager/Server")->item(0)->nodeValue);
//$clientManagerClientCode = trim($xpath->query("//ClientManager/ClientCode")->item(0)->nodeValue);

$rootPath = $host . $rootPathRemote;

if (stripos($host, "localhost") !== false) {
    $rootPath = $host . $rootPathLocal;
}

$rootPathAbs = realpath(dirname($_SERVER["SCRIPT_FILENAME"]) . "/../");

$_SESSION["ROOT_PATH"] = $rootPath;
$_SESSION["ROOT_PATH_ABS"] = $rootPathAbs;
$_SESSION["KHRONUS_FRAMEWORK_PATH"] = (stripos($host, "localhost") !== false) ? "Z:/vsisolucoes/khronus-framework/php/v3.0.2/" : $rootPathAbs;

try {

//    if (!isset($_SESSION['APP_INITIALIZED']) || stripos($host, "localhost") !== false || $_SESSION['CURRENT_PATH'] != $_SESSION["ROOT_PATH"]) {
//
//        //
//        // 
//        //
//        $clientManagerClientCode = trim($xpath->query("//ClientManager/ClientCode")->item(0)->nodeValue);
//
//        $ctx = stream_context_create(array('http' =>
//            array(
//                'timeout' => 5, //1200 Seconds is 20 Minutes
//            )
//        ));
//
//        $servers = $xpath->query("//ClientManager/Servers")->item(0);
//
//        for ($x = 0; $x < $servers->childNodes->length - 1; $x++) {
//
//            if ($servers->childNodes[$x]->nodeType == 1) {
//
//                $clientManagerServer = trim($servers->childNodes[$x]->nodeValue);
//
//                $json = file_get_contents("$clientManagerServer/index.php?endpoint=aplicacao&filter=GERAN_CODIGO='$clientManagerClientCode'", false, $ctx);
//
//                if ($json) {
//                    break;
//                }
//            }
//        }
//        
//        if(!$json) {
//            throw new Exception("Falha ao conectar no servidor");
//        }
//
//        $obj = json_decode($json);
//
//        $_SESSION["GERAN_BD_SERVIDOR"] = $obj->status_message[0]->GERAN_BD_SERVIDOR;
//        $_SESSION["GERAN_BD_PORTA"] = $obj->status_message[0]->GERAN_BD_PORTA;
//        $_SESSION["GERAN_BD_NOME"] = $obj->status_message[0]->GERAN_BD_NOME;
//        $_SESSION["GERAN_BD_USUARIO"] = $obj->status_message[0]->GERAN_BD_USUARIO;
//        $_SESSION["GERAN_BD_SENHA"] = $obj->status_message[0]->GERAN_BD_SENHA;
//        $_SESSION["GERAN_BD_OPCOES"] = $obj->status_message[0]->GERAN_BD_OPCOES;
//        
//        $_SESSION['APP_INITIALIZED'] = true;
//        
//        $_SESSION['CURRENT_PATH'] = $_SESSION["ROOT_PATH"];
//    }
    
    $_SESSION["GERAN_BD_SERVIDOR"] = "mysql.vsisolucoes.com.br";
    $_SESSION["GERAN_BD_PORTA"] = "3306";
    $_SESSION["GERAN_BD_NOME"] = "vsisolucoe153";
    $_SESSION["GERAN_BD_USUARIO"] = "vsisolucoe153";
    $_SESSION["GERAN_BD_SENHA"] = "V123456";
    $_SESSION["GERAN_BD_OPCOES"] = "charset=utf8";
    
    //
    // API
    //
    require_once "loader.php";
    
    require $_SESSION["KHRONUS_FRAMEWORK_PATH"] . "/_api/slim/Slim/Slim.php";

    $apiController = new ApiController();
    $apiController->run();
    
} catch (Exception $ex) {
    echo($ex);
}
