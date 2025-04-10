# Установщик RpaJsonDllStudio для Windows

Этот каталог содержит скрипт для создания установщика RpaJsonDllStudio для операционной системы Windows с использованием Inno Setup.

## Требования

1. **Inno Setup**:
   - Скачайте и установите [Inno Setup](https://jrsoftware.org/isdl.php) (версия 6.0 или выше)
   - Рекомендуется использовать Unicode версию

2. **Сборка приложения**:
   - Перед созданием установщика необходимо собрать релизную версию приложения:
   ```
   dotnet publish ..\..\src\RpaJsonDllStudio\RpaJsonDllStudio.csproj -c Release -r win-x64 --self-contained
   ```

## Создание установщика

1. Запустите Inno Setup Compiler
2. Откройте файл `setup_windows.iss`
3. Выберите `Build` → `Compile` (или нажмите F9)
4. Готовый установщик будет создан в подкаталоге `Output`

## Особенности установщика

- Проверка наличия .NET 9.0 (Runtime или SDK)
- Проверка версии Windows (требуется Windows 10 или новее)
- Многоязычная поддержка (русский и английский)
- Создание ярлыков в меню Пуск и на рабочем столе (опционально)
- Возможность запуска приложения сразу после установки

## Параметры командной строки

Установщик поддерживает следующие параметры командной строки:

- `/SKIPDOTNETCHECK` - пропустить проверку наличия .NET Runtime/SDK
  ```
  RpaJsonDllStudio_Windows_Setup_v1.0.0.exe /SKIPDOTNETCHECK
  ```

## Настройка

При необходимости отредактируйте файл `setup_windows.iss`, чтобы изменить:

- Версию приложения (`MyAppVersion`)
- Издателя (`MyAppPublisher`)
- URL репозитория/сайта (`MyAppURL`)
- Минимальную версию Windows (`WindowsMinVersion`)

## Сборка установщика из командной строки

Можно собрать установщик из командной строки:

```
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" setup_windows.iss
``` 