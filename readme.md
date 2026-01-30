# .NET SistemaBillar 🧱

Plantilla base para proyectos .NET que te permite iniciar un nuevo proyecto con una estructura organizada y buenas prácticas.

## 📝 Descripción

Este repositorio es una **plantilla de proyecto para .NET** que puedes usar como punto de partida para construir aplicaciones con varios niveles de arquitectura (Aplicación, Dominio, Infraestructura, API, Cliente, etc.). Está pensado para acelerar la creación de proyectos consistentes y escalables.

## 🚀 ¿Qué incluye?

- 🗂 Estructura modular con carpetas separadas por capas (App, Domain, Infrastructure, WebApi, WebClient).
- 📦 Solución base (`SistemaBillar.sln`).
- 🔧 Archivos de configuración y `.gitignore`.
- Código listo para extender según tus necesidades.

## 🧰 Requisitos

Antes de usar esta plantilla, asegúrate de tener instalado:

- .NET SDK (versión recomendada o superior)
- Un IDE compatible con .NET (Visual Studio, VS Code, Rider, etc.)

## 📥 Cómo usar esta plantilla

### ⭐ Opción 1: Usar como plantilla

1. En la página del repositorio, haz clic en **Use this SistemaBillar**.
2. Crea un nuevo repositorio a partir de la plantilla.
3. Clona tu nuevo repositorio y comienza a trabajar.

### 📌 Opción 2: Clonar el repositorio

```bash
git clone https://github.com/nahuelXZV/.net-SistemaBillar.git
cd .net-SistemaBillar
```

Abre la solución `SistemaBillar.sln` en tu IDE favorito.

# 🔄 Renombrar el proyecto
Para renombrar el proyecto, puedes usar el script `rename-project.ps1` incluido en la raíz del repositorio. Este script te permitirá cambiar el nombre del proyecto en todos los archivos y carpetas relevantes.
```powershell
.\rename-project.ps1 -OldName "SistemaBillar" -NewName "NuevoNombre"
```
Este script realiza las siguientes acciones:
1. Cambia el nombre de las carpetas que contienen el nombre antiguo.
2. Reemplaza el texto dentro de los archivos para reflejar el nuevo nombre del proyecto

