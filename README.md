# KPMG API GAME

### || Para executar, basta dar play no projeto pelo visual studio, pois os serviços utilizados estão na nuvem e estão previamente configurados no projeto||

## O projeto está estruturado em 3 grandes partes
- **kpmg-api-game** - Api onde expõe 2 endpoints apenas
- **kpmg-core** - Class Library onde tem classes bases para o projeto evitar redundâncias e garantindo o princípio DRY
- **kpmg-worker** - Console Application responsável por rodar de tempos em tempos, para verificar se existe algo no Cache do **Redis** para persistir no **MongoDB**, esse tempo de execução é totalmente configurável, basta ir no arquivo **appsettings.json** do projeto e alterar a propriedade **ScheduleInMilliseconds** para o valor desejado em **Milissegundos**

## Features

- Registra o resultado de um jogo, com as seguintes informações (**ID do jogador, ID do jogo, número de pontos ganhos (positivos ou negativos) e Timestamp**)
- Devolve um ranking dos jogadores e suas pontuações com as seguintes informações (**data em que o balanço de pontos do jogador foi atualizado pela última vez (usando o fuso horário do servidor de aplicação**))

## Tecnologias Utilizadas

Tecnologias utilizadas e seus motivos:

- [Dotnet] - Free. Cross-platform. Open source. A developer platform for building all apps.
    - Foi utilizado Dotnet Core 3.1 LTS, devido ao requisíto do teste ser Dotnet
- [RedisLabs] - Rediscover the power of real‑time data
    - Foi utilizado o Redis para suprir a necessidade de gerenciar os dados em memória, sendo utilizado a versão online para garantir facilidade na execução da aplicação
- [MongoDb-Atlas] - The most innovative cloud database service on the market, with unmatched data distribution and mobility across AWS, Azure, and Google Cloud, built-in automation for resource and workload optimization, and so much more.
    - Foi utilizado o MongoDB para suprir a necessidade de persistir os dados e se tratando dos dados ser simples, achei melhor usar a abordagem NoSQL ao invés de SQL, como SQL Server, utilizei a versão online para garantir facilidade na execução da aplicação


[//]: # (These are reference links used in the body of this note and get stripped out when the markdown processor does its job. There is no need to format nicely because it shouldn't be seen. Thanks SO - http://stackoverflow.com/questions/4823468/store-comments-in-markdown-syntax)
   [Dotnet]: <https://dotnet.microsoft.com>
   [RedisLabs]: <https://redislabs.com>
   [MongoDb-Atlas]: <https://www.mongodb.com/cloud/atlas>
