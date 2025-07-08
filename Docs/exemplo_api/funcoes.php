<?php

function auth($url, $chave_api, $codigo_cliente, $usuario, $senha)
{
    $url_autenticacao = $url . 'headers_authorization?cliente='.$codigo_cliente.'&nome='.$usuario.'&senha='.$senha.'';

    $retorno = curl($url_autenticacao);
    $retorno = json_decode($retorno['resposta']);

    $header_retorno = (array) $retorno->Headers;
    $token = $header_retorno['X-Auth-Token'];
    $auth = $header_retorno['Authorization'];

    $header = ['Accept' => 'application/json', 'X-Auth-Token' => $token, 'Authorization' => $auth];

    return $header;
}

function buscarCliente($url, $header, $chave_api, $data = [])
{
    $cliente = curl($url .'/'.  $chave_api, $data, 'GET', $header);

    $cliente = json_decode($cliente['resposta']);
    return $cliente;
}

function inserirCliente($url, $header, $chave_api, $data = [])
{
    $cliente = curl($url .'/'.  $chave_api, $data, 'POST', $header);

    $cliente = json_decode($cliente['resposta']);
    return $cliente;
}

############################################################################
##CURL
function curl($url, $data=[], $method = 'POST', $header =[], $teste = false)
{
        $dir = dirname(__FILE__) . '/cookie.txt';

        $curl = curl_init();

        $fields = http_build_query($data);

        switch ($method) {
            case 'GET':
                if($data){
                    $url = $url.'?'.$fields;
                }
                break;
            case 'POST':
                if($data){
                    curl_setopt($curl, CURLOPT_POSTFIELDS, $fields);
                }
                curl_setopt($curl, CURLOPT_POST, 1);
                break;
            case 'PUT':
                if($data){
                    curl_setopt($curl, CURLOPT_POSTFIELDS, $fields);
                }
                curl_setopt($curl, CURLOPT_CUSTOMREQUEST, "PUT");
                break;
            case 'DELETE':
                curl_setopt($curl, CURLOPT_CUSTOMREQUEST, 'DELETE');
                break;
        }

        curl_setopt($curl, CURLOPT_COOKIEFILE, $dir);
        curl_setopt($curl, CURLOPT_COOKIEJAR, $dir);
        // curl_setopt($curl, CURLOPT_COOKIEFILE, $dir);


        if (count($header) > 0) {
            curl_setopt($curl, CURLOPT_HTTPHEADER, $header);
        }

        curl_setopt($curl, CURLOPT_URL, $url);
        curl_setopt($curl, CURLOPT_TIMEOUT, 30);
        curl_setopt($curl, CURLOPT_CONNECTTIMEOUT, 30);
        curl_setopt($curl, CURLOPT_RETURNTRANSFER, true);

        $retorno  = curl_exec($curl);
        $httpcode = curl_getinfo($curl, CURLINFO_HTTP_CODE);
        curl_close($curl);
        return ['resposta'=> $retorno, 'status'=> $httpcode];
}

?>