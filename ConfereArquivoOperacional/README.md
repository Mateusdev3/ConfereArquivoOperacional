# ConfereOperacional (.NET Console)

Programa console desenvolvido em **C# / .NET** para validação operacional de pastas por data, realizando conferências automáticas em arquivos `.tac` de múltiplas operadoras e gerando um **relatório detalhado em log**.

---

## 🧠 O que o programa faz

✔ Valida a quantidade de arquivos `.tac` por operadora (esperado: **3**)  
✔ Verifica a existência do arquivo **`operacional-painel.tac`**  
✔ Abre o `.tac` como **ZIP**  
✔ Confere a presença dos arquivos obrigatórios dentro do painel  
✔ Exibe resultado no console com cores e ícones  
✔ Gera um **log completo em `.txt`** por data analisada  

---

## 🧪 Arquivos obrigatórios no painel

```
buslines.upex
confgps.upex
messages.upex
motorista.upex
tripschedule.upex
version.txt
```

---

## 📂 Estrutura esperada

```
ConfereOperacional/
 ├─ ConfereOperacional.exe
 ├─ 20260119/
 │   ├─ 001/
 │   │   ├─ arquivo1.tac
 │   │   ├─ arquivo2.tac
 │   │   └─ operacional-painel.tac
 │   ├─ 002/
 │   └─ ...
```

> A pasta da data deve ficar **no mesmo diretório do executável**.

---

## 🖥️ Exemplo de saída no console

```
▶ Operadora 001
 ✓ TACs: 3/3
 ✓ Painel: OK

▶ Operadora 002
 ✗ TACs: 2/3
 ✗ Painel: FALTANDO (1)
```

---

## 📝 Log gerado

📄 `{DATA}_logs.txt`

Inclui:
- Data/hora da execução
- Status por operadora
- Lista de TACs encontrados
- Arquivos ausentes no painel
- Resumo geral

---

## 🚀 Como executar

### Pré-requisitos
- **.NET SDK 6 ou superior**

Verificar:
```
dotnet --version
```

### Executar
```
dotnet run
```

### Build
```
dotnet build -c Release
```

---

## 🛠️ Tecnologias utilizadas

![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Console](https://img.shields.io/badge/Console_App-000000?style=for-the-badge&logo=windows-terminal&logoColor=white)

---

## 👤 Autor

Mateus Esteves

---

## 📄 Licença

MIT License

Copyright (c) 2026 Mateus Esteves

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND.
