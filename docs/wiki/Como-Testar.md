# Como testar

Duas frentes: os testes automatizados (rodam em segundos, sem precisar subir
nada) e um roteiro manual pra validar a aplicação de ponta a ponta depois de
rodá-la (veja [Instalação e Deploy](Instalacao-e-Deploy.md) primeiro).

## 1. Testes automatizados

```bash
dotnet test
```

39 testes (xUnit + Moq), organizados por camada:

| Camada | O que é testado | Arquivos |
|---|---|---|
| `Domain` | Regras de agregação (`WeatherStatsCalculator`): agrupamento por dia, ícone representativo, estatísticas do dia atual, casos de borda (datas invertidas, sem leituras) | `WeatherStatsCalculatorTests` |
| `Domain` | Catálogos de cidade: contagem, ids únicos, `FindById` case-insensitive, união sem duplicar Curitiba | `BrazilianCapitalsTests`, `CuritibaMetroRegionTests`, `TrackedCitiesTests` |
| `Infrastructure` | Repositório EF Core (InMemory): grava, filtra por cidade/período, ordena | `EfWeatherRecordRepositoryTests` |
| `Infrastructure` | Cliente OpenWeatherMap: parse da resposta, tratamento de erro HTTP, chave ausente, exceção de rede (via `HttpMessageHandler` falso — sem chamada de rede real) | `OpenWeatherMapClientTests` |
| `Api` | Controllers (`WeatherController`, `CitiesController`): validação de parâmetros, contagem/forma da resposta | `WeatherControllerTests`, `CitiesControllerTests` |
| `Web` | `HomeController`: cidade padrão, resolução por id, URLs da API repassadas pra view | `HomeControllerTests` |

Não há teste de UI automatizado (Selenium/Playwright) — o roteiro manual
abaixo cobre essa parte.

## 2. Roteiro manual

Com a `Api` e o `Web` rodando (dois terminais, ver
[Instalação e Deploy](Instalacao-e-Deploy.md#3-restaurar-compilar-e-rodar)):

### 2.1 Coleta de dados

1. Assim que a `Api` sobe, o log deve mostrar `Iniciando ciclo de coleta
   climática para 55 cidades` seguido de `Ciclo de coleta concluído: 55/55
   cidades` em poucos segundos (com a chave da OpenWeatherMap configurada).
2. Sem chave configurada, o log mostra avisos ("ApiKey não configurada")
   pra cada cidade e conclui `0/55` — a aplicação não deve travar nem lançar
   exceção.

### 2.2 Dashboard (site)

1. Abrir `http://localhost:5170`. A cidade padrão deve ser **Curitiba**, com
   dados aparecendo em poucos segundos (assim que a primeira coleta salva no
   banco).
2. **Seletor de cidade**: confirmar dois grupos no dropdown — "Região
   Metropolitana de Curitiba" (29 opções) e "Capitais" (27, sem repetir
   Curitiba). Trocar de cidade deve atualizar cartão atual, estatísticas,
   tira de dias e os dois gráficos.
3. **Tira de destaque**: por padrão mostra as 9 cidades que fazem fronteira
   com Curitiba, cada uma com temperatura atual. Clicar num chip troca a
   cidade selecionada. Clicar em "Ver capitais" alterna pra 8 capitais em
   destaque, e o botão muda para "Ver região metropolitana".
4. **Filtros de data**: mudar "De"/"Até" e clicar "Aplicar" deve atualizar
   os gráficos e a tira de dias para o novo período; as estatísticas "do dia
   atual" não devem mudar (são sempre hoje, independente do filtro).
5. **Toggle °C/°F**: alternar deve converter todos os valores exibidos
   (cartão atual, estatísticas, gráficos, tira de destaque) sem recarregar a
   página nem refazer a requisição.
6. **Gráficos**: com pelo menos um dia de dados coletados, os dois gráficos
   (temperatura mín/média/máx e umidade/vento) devem renderizar; sem dados
   no período, aparece a mensagem "Ainda não há dados coletados" no lugar
   dos gráficos.
7. **Cena de fundo**: deve mudar conforme a condição da cidade selecionada
   (sol, nuvens, chuva, tempestade com relâmpago, ou céu estrelado à noite).
   Pra forçar cada cena sem esperar o clima mudar de verdade, no console do
   navegador: `WeatherScene.render('11d')` (tempestade), `'01n'` (noite
   limpa), `'10d'` (chuva), `'03d'` (nublado), etc.

### 2.3 Responsividade

Redimensionar a janela (ou usar o modo responsivo do DevTools) para larguras
de smartphone (~375px) e tablet (~768px): sem barra de rolagem horizontal, a
tira de destaque e a tira de dias devem rolar horizontalmente, e os cartões
de estatística devem reorganizar em menos colunas.

### 2.4 API / Swagger

1. Abrir `http://localhost:5282/swagger` (ou a porta https, `7222`).
2. Testar `GET /api/cities` — deve devolver 55 cidades.
3. Testar `GET /api/weather/data` com `city=curitiba`, `start`/`end` de hoje
   — deve devolver `dailySeries` e `today` preenchidos (com dados já
   coletados).
4. Testar `GET /api/weather/data` com uma cidade inexistente (ex.:
   `city=atlantis`) — deve devolver `400 Bad Request`.
5. Confirmar que os dois endpoints aparecem documentados com descrição
   (vêm dos comentários XML do código).

### 2.5 Erros esperados (não são bugs)

- Abrir só o `Web` sem a `Api` rodando: o dashboard carrega a página, mas os
  cartões/gráficos mostram estado de erro (a `Api` não respondeu) — nunca
  uma tela em branco ou um erro 500 do site.
- Selecionar uma cidade sem nenhuma leitura ainda: aparece "Sem leitura
  registrada hoje para [cidade] ainda" no lugar do cartão de clima atual.
