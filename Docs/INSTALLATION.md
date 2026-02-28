# Installation MESSIEMATALA POS (PC Client)

## 1) Prérequis
- Windows 10/11
- .NET Framework 4.8
- SQL Server Express (recommandé)

## 2) Installer les prérequis (si pas déjà fait)
1. Lancer `01_Prerequis/dotNetFx48.exe`
2. Installer SQL Server Express `01_Prerequis/SQLEXPR_x64.exe`
   - Instance : `SQLEXPRESS`

## 3) Installer l’application
1. Lancer `02_Installation/Setup.exe` (ou .msi)
2. Terminer l'installation

## 4) Première ouverture (Configuration système)
1. Serveur : `.\SQLEXPRESS`
2. Authentification : Windows (recommandé)
3. Base : `BoutiqueDB_NomClient`
4. Cliquer **Créer base**
5. Cliquer **Tester connexion**
6. Cliquer **Enregistrer**

## 5) Connexion
- Utilisateur : MESSIE
- Mot de passe : 1234
