# Monitoramento sobre a cota��o de uma a��o
Aplica��o de monitoramento de valor sobre a cota��o de uma a��o solicitada.
Envia e-mail caso seja recomendado comprar ou vender a a��o escolhida para monitoramento.

## Uso
Para executar o programa � necess�rio informar os par�metros: a��o, pre�o de venda e pre�o de compra, al�m de preencher o arquivo de configura��o: config.xml
Par�metros: A�AO PRECO_VENDA PRECO_COMPRA
Exemplo:
```sh
.\desafio-bt.exe PETR4 40.0 5.1
```

## Arquivo de configura��o xml
Prencha o arquivo com os dados de cada campo
 - tokenApi: token da API
 - destinationEmail: e-mail de destino para o aviso
 - host: host do server SMTP
 - port: porta do server SMTP
 - username: username ou email do usu�rio cadastrado no servidor
 - password: senha do usu�rio cadastrado no servidor
