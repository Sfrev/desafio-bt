# Monitoramento sobre a cotação de uma ação
Aplicação de monitoramento de valor sobre a cotação de uma ação solicitada.
Envia e-mail caso seja recomendado comprar ou vender a ação escolhida para monitoramento.

## Uso
Para executar o programa é necessário informar os parâmetros: ação, preço de venda e preço de compra, além de preencher o arquivo de configuração: config.xml
Parâmetros: AÇAO PRECO_VENDA PRECO_COMPRA
Exemplo:
```sh
.\desafio-bt.exe PETR4 40.0 5.1
```

## Arquivo de configuração xml
Prencha o arquivo com os dados de cada campo
 - tokenApi: token da API
 - destinationEmail: e-mail de destino para o aviso
 - host: host do server SMTP
 - port: porta do server SMTP
 - username: username ou email do usuário cadastrado no servidor
 - password: senha do usuário cadastrado no servidor
