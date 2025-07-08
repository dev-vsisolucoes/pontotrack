<title> API SGR </title>
<?php
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
include 'funcoes.php';

$url            = 'https://sgr.hinova.com.br/sgr/sgrv2_api/service_api/';
$chave_api      = '688472668f481b3efbddb0bfbff99cf6';
$codigo_cliente = 3569;
$usuario        = 'moskit';
$senha          = '2SI7WG';

#authFnpap$!z
$header = auth($url, $chave_api, $codigo_cliente, $usuario, $senha);
#auth
// print_r($header); exit();
#buscar_cliente
$data = [
    'cod_situacao_cliente' => '1',
    'nome_cliente' => 'cliente teste'
    // 'cpf_cliente' => 123456
];
$url  = $url . 'servicos/inserir_cliente';

$cliente = inserirCliente($url, $header, $chave_api, $data);
echo '<pre>';
print_r($cliente); exit();
#buscar_cliente


?>