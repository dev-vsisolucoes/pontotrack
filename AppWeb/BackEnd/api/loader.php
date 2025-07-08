<?php

spl_autoload_register(function($class) {
    $class = str_replace("\\", "/", $class);
            
    $list = array(
        
        $_SESSION["KHRONUS_FRAMEWORK_PATH"] . "/_api/khronus-3.0.0/$class.php",
        $_SESSION["KHRONUS_FRAMEWORK_PATH"] . "/_api/khronus-3.0.0/util/$class.php",
        $_SESSION["KHRONUS_FRAMEWORK_PATH"] . "/_api/omie/$class.php",
        $_SESSION["KHRONUS_FRAMEWORK_PATH"] . "/_api/tcpdf/$class.php",
        $_SESSION["KHRONUS_FRAMEWORK_PATH"] . "/_api/phpmailer/$class.php",
        $_SESSION["KHRONUS_FRAMEWORK_PATH"] . "/_api/dompdf/src/" . substr($class, 7) . ".php",
        $_SESSION["KHRONUS_FRAMEWORK_PATH"] . "/_api/phpoffice/$class.php",
        $_SESSION["KHRONUS_FRAMEWORK_PATH"] . "/_api/finediff/$class.php",
        $_SESSION["KHRONUS_FRAMEWORK_PATH"] . "/_api/woo/$class.php",
        $_SESSION["ROOT_PATH_ABS"] . "/api/controllers/$class.php",
        $_SESSION["ROOT_PATH_ABS"] . "/api/models/$class.php",
        
        $_SESSION["ROOT_PATH_ABS"] . "/$class.php",
    );

    foreach ($list as $k) {
        if (file_exists($k)) {
            
            require_once $k;
            
            break;
        }
    }
});