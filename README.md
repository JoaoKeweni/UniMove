# UniMove — Sistema Distribuído de Caronas Universitárias

Aplicação de demonstração de **Sistemas Distribuídos com Sockets TCP**, inspirada no BlaBlaCar, focada em caronas entre campi universitários de Sergipe.

Toda a regra de negócio fica no **servidor**; os **clientes** apenas enviam solicitações e exibem resultados. A comunicação usa **mensagens JSON** sobre **TCP**.

## Arquitetura

```
Cliente (Windows Forms)
        │  Socket TCP + JSON
        ▼
Servidor (Console)
        │  Regra de negócio
        ▼
SQLite local (unimove.db)
```

- **UniMove.Shared** — modelos e protocolo (`Carona`, `Reserva`, `Usuario`, `Mensagem`, `Operacao`, `Campi`, `CaronaComReservas`).
- **UniMove.Server** — servidor TCP multicliente (`SocketServidor`, `ClienteHandler`), serviços (`UsuarioService`, `CaronaService`, `ReservaService`) e persistência SQLite (`Banco`).
- **UniMove.Client** — interface Windows Forms (`TelaLogin`, `TelaPrincipal`, `TelaPublicar`, `TelaBuscar`, `TelaReservas`) e o `SocketCliente`.

## Funcionalidades

- **Login/Registro simples** (nome + senha) persistido em SQLite.
- **Publicar carona**: motorista informa a origem (texto livre) e o **campus de destino**.
- **Buscar caronas**: o passageiro escolhe o **campus aonde quer chegar** e vê as caronas para lá, **independente da origem** do motorista.
- **Reservar vaga**: diminui uma vaga e registra a reserva (transação atômica).
- **Painel de Reservas**: mostra, por carona, quem ofereceu e quem reservou.

### Campi atendidos

Laranjeiras · São Cristóvão · Lagarto · Itabaiana · Nossa Senhora da Glória

## Como executar

Requer **.NET 8 SDK** (Windows, por causa do Windows Forms).

```bash
# 1) Servidor (porta 5000)
dotnet run --project UniMove.Server

# 2) Cliente (pode abrir vários)
dotnet run --project UniMove.Client
```

## Protocolo

Cada requisição é uma linha JSON com o envelope `Mensagem`:

```json
{ "Operacao": 0, "Dados": "{...}" }
```

Operações: `Publicar`, `Buscar`, `Reservar`, `Resposta`, `Erro`, `Reservas`, `Registrar`, `Login`.

## Tecnologias

C# · .NET 8 · Windows Forms · TCP Socket · System.Text.Json · Microsoft.Data.Sqlite
