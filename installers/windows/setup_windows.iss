;
; Скрипт установки RpaJsonDllStudio для Windows
; Создан с использованием Inno Setup
; Требуется .NET 9.0 (Runtime или SDK)
;

#define MyAppName "RpaJsonDllStudio"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Alexey Egorov"
#define MyAppURL "https://github.com/themrdjek/RpaJsonDllStudio"
#define MyAppExeName "RpaJsonDllStudio.exe"
#define DotNetRuntimeVersion "9.0"
#define DotNetRuntimeURL "https://dotnet.microsoft.com/download/dotnet/9.0"
#define WindowsMinVersion "10.0"

[Setup]
; AppId уникально идентифицирует приложение
AppId={{37A9E2F8-3F15-4D1B-B82C-C8E0D8D76410}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\..\LICENSE
OutputDir=Output
OutputBaseFilename=RpaJsonDllStudio_Windows_Setup_v{#MyAppVersion}
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
WizardStyle=modern
; Установка значка установщика
SetupIconFile=..\..\src\RpaJsonDllStudio\Assets\app-icon.ico
; Минимальная версия Windows
MinVersion={#WindowsMinVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
Source: "..\..\src\RpaJsonDllStudio\bin\Release\net9.0\win-x64\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\src\RpaJsonDllStudio\bin\Release\net9.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Проверяет, начинается ли строка с указанного префикса
function StartsWith(const Text, Prefix: string): Boolean;
begin
  Result := Copy(Text, 1, Length(Prefix)) = Prefix;
end;

// Функция проверки наличия .NET Runtime
function IsDotNetRuntimeInstalled: Boolean;
var
  Key: String;
  Install: Cardinal;
  ReleasesKey: String;
  RuntimeVersion: String;
begin
  Result := False;
  
  // Проверка .NET Runtime через реестр x64
  Key := 'SOFTWARE\Microsoft\NET Core\Setup\InstalledVersions\x64\sharedhost';
  if RegQueryDWordValue(HKLM, Key, 'Version', Install) then
  begin
    // Проверка конкретной версии через Microsoft.NETCore.App
    ReleasesKey := 'SOFTWARE\Microsoft\NET Core\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App';
    if RegQueryStringValue(HKLM, ReleasesKey, 'Version', RuntimeVersion) then
    begin
      // Проверка, что версия начинается с нужной версии
      if StartsWith(RuntimeVersion, '{#DotNetRuntimeVersion}') then
      begin
        Result := True;
      end;
    end;
  end;
end;

// Функция для логирования в отладочный режим
procedure Log(Message: string);
begin
  if IsUninstaller() = false then
  begin
    if Length(Message) > 0 then
      MsgBox(Message, mbInformation, MB_OK);
  end;
end;

// Расширенная функция проверки наличия .NET SDK
function IsDotNetSdkInstalled: Boolean;
var
  Key: String;
  SdkKey: String;
  SdkVersion: String;
  SdkNames: TArrayOfString;
  I, J: Integer;
  DotNetPath: String;
  // Добавляем больше ключей для поиска SDK
  SDKRegistryKeys: array [0..4] of String;
begin
  Result := False;
  
  // Инициализация массива ключей реестра для поиска SDK
  SDKRegistryKeys[0] := 'SOFTWARE\Microsoft\NET Core\Setup\InstalledVersions\x64\sdk';
  SDKRegistryKeys[1] := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sdk';
  SDKRegistryKeys[2] := 'SOFTWARE\Microsoft\dotnet\Setup\InstalledVersions\x64\sdks';
  SDKRegistryKeys[3] := 'SOFTWARE\Microsoft\dotnet\Setup\InstalledVersions\x64\sdk';
  SDKRegistryKeys[4] := 'SOFTWARE\dotnet\SDK\Versions';

  // Проверка всех возможных ключей реестра
  for I := 0 to 4 do
  begin
    if RegValueExists(HKLM, SDKRegistryKeys[I], 'Version') then
    begin
      if RegQueryStringValue(HKLM, SDKRegistryKeys[I], 'Version', SdkVersion) then
      begin
        // Для отладки можно вывести найденную версию
        // Log('Найден SDK: ' + SdkVersion + ' в ' + SDKRegistryKeys[I]);
        
        // Проверить, содержит ли версия нужную нам
        if Pos('{#DotNetRuntimeVersion}', SdkVersion) > 0 then
        begin
          Result := True;
          Exit;
        end;
      end;
    end;
  end;

  // Дополнительная проверка - поиск исполняемого файла dotnet.exe
  if not Result then
  begin
    // Проверка наличия dotnet.exe в системных путях
    if RegQueryStringValue(HKLM, 'SOFTWARE\dotnet\Setup', 'InstallDir', DotNetPath) then
    begin
      // Log('Найден путь .NET: ' + DotNetPath);
      if FileExists(DotNetPath + '\dotnet.exe') then
      begin
        // Если файл существует, считаем что .NET установлен
        // Это не очень точная проверка, но как запасной вариант
        Result := True;
        Exit;
      end;
    end;
  end;

  // Еще один способ - проверка через список установленных SDK
  if not Result then
  begin
    for I := 0 to 4 do
    begin
      if RegGetSubkeyNames(HKLM, SDKRegistryKeys[I], SdkNames) then
      begin
        // Перебираем все имена подключей
        for J := 0 to GetArrayLength(SdkNames) - 1 do
        begin
          // Log('Найден подключ SDK: ' + SdkNames[J]);
          if Pos('{#DotNetRuntimeVersion}', SdkNames[J]) > 0 then
          begin
            Result := True;
            Exit;
          end;
        end;
      end;
    end;
  end;
  
  // Проверка наличия dotnet в Program Files
  if not Result then
  begin
    if DirExists(ExpandConstant('{pf64}\dotnet')) then
    begin
      Result := True;
      Exit;
    end;
  end;
end;

// Проверка наличия либо .NET Runtime, либо .NET SDK
function IsDotNetInstalled: Boolean;
begin
  Result := IsDotNetRuntimeInstalled or IsDotNetSdkInstalled;
end;

// Проверка версии Windows
function IsWindowsVersionSupported: Boolean;
var
  Version: TWindowsVersion;
begin
  GetWindowsVersionEx(Version);
  // Windows 10 имеет основную версию 10
  // Преобразуем строку в число для сравнения
  Result := Version.Major >= StrToInt(Copy('{#WindowsMinVersion}', 1, Pos('.', '{#WindowsMinVersion}') - 1));
end;

// Проверяем наличие .NET Runtime или SDK перед установкой
function InitializeSetup: Boolean;
var
  ErrorCode: Integer;
  DotNetNeeded: Boolean;
  Result1: Boolean;
  SkipDotNetCheck: Boolean;
begin
  Result := True;
  
  // Проверка параметров командной строки для пропуска проверки
  SkipDotNetCheck := (ParamStr(1) = '/SKIPDOTNETCHECK') or (ParamStr(2) = '/SKIPDOTNETCHECK');
  
  // Проверка версии Windows
  if not IsWindowsVersionSupported then
  begin
    if ActiveLanguage = 'russian' then
    begin
      MsgBox('Это приложение требует Windows 10 или новее. Пожалуйста, обновите операционную систему.', mbError, MB_OK);
    end else begin
      MsgBox('This application requires Windows 10 or newer. Please update your operating system.', mbError, MB_OK);
    end;
    Result := False;
    Exit;
  end;
  
  // Проверка .NET - только если не указан параметр пропуска проверки
  if not SkipDotNetCheck then
  begin
    DotNetNeeded := not IsDotNetInstalled;
    
    if DotNetNeeded then
    begin
      if ActiveLanguage = 'russian' then
      begin
        Result1 := MsgBox('Для работы приложения требуется .NET {#DotNetRuntimeVersion} (Runtime или SDK).' + #13#10 + 
                        'Он не установлен на вашем компьютере.' + #13#10 + #13#10 +
                        'Хотите загрузить .NET {#DotNetRuntimeVersion} сейчас?', 
                        mbConfirmation, MB_YESNO) = IDYES;
      end else begin
        Result1 := MsgBox('This application requires .NET {#DotNetRuntimeVersion} (Runtime or SDK).' + #13#10 + 
                        'It is not installed on your computer.' + #13#10 + #13#10 +
                        'Do you want to download .NET {#DotNetRuntimeVersion} now?', 
                        mbConfirmation, MB_YESNO) = IDYES;
      end;
      
      if Result1 then
      begin
        ShellExec('open', '{#DotNetRuntimeURL}', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
        if ActiveLanguage = 'russian' then
        begin
          Result := MsgBox('Пожалуйста, запустите этот установщик снова после установки .NET {#DotNetRuntimeVersion}.' + #13#10 +
                         'Хотите продолжить установку без .NET?' + #13#10 + #13#10 +
                         'Подсказка: Вы можете запустить установщик с параметром /SKIPDOTNETCHECK, чтобы пропустить эту проверку.',
                         mbConfirmation, MB_YESNO) = IDYES;
        end else begin
          Result := MsgBox('Please run this installer again after installing .NET {#DotNetRuntimeVersion}.' + #13#10 +
                         'Do you want to continue the installation without .NET?' + #13#10 + #13#10 +
                         'Tip: You can run the installer with /SKIPDOTNETCHECK parameter to skip this check.',
                         mbConfirmation, MB_YESNO) = IDYES;
        end;
      end
      else
      begin
        if ActiveLanguage = 'russian' then
        begin
          Result := MsgBox('Эта программа может некорректно работать без .NET {#DotNetRuntimeVersion}.' + #13#10 +
                         'Хотите продолжить установку?' + #13#10 + #13#10 +
                         'Подсказка: Вы можете запустить установщик с параметром /SKIPDOTNETCHECK, чтобы пропустить эту проверку.',
                         mbConfirmation, MB_YESNO) = IDYES;
        end else begin
          Result := MsgBox('This application may not work correctly without .NET {#DotNetRuntimeVersion}.' + #13#10 +
                         'Do you want to continue with the installation?' + #13#10 + #13#10 +
                         'Tip: You can run the installer with /SKIPDOTNETCHECK parameter to skip this check.',
                         mbConfirmation, MB_YESNO) = IDYES;
        end;
      end;
    end;
  end;
end; 