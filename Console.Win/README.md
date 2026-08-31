# Remotely Console

Controlador desktop do Windows para o servidor Remotely.

## Como funciona

O programa abre o painel do hub dentro de uma janela nativa usando o Microsoft
Edge WebView2. A sessão, os cookies e as janelas de controle remoto ficam
isolados no perfil local do aplicativo.

Na primeira execução, o endereço padrão é `https://localhost:5001`. Use
**Arquivo > Configurar servidor** para informar o endereço do PC principal ou
o endereço privado usado fora da rede local.

## Executar durante o desenvolvimento

    dotnet run --project Console.Win/Console.Win.csproj

## Gerar uma versão para Windows x64

    dotnet publish Console.Win/Console.Win.csproj -c Release -p:PublishProfile=win-x64

O Windows 10/11 normalmente já possui o WebView2 Runtime. Caso ele não esteja
instalado, o aplicativo mostra uma mensagem informando o componente ausente.
