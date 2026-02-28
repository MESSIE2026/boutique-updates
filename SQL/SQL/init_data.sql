IF NOT EXISTS (SELECT 1 FROM dbo.Employes WHERE NomUtilisateur = 'admin')
BEGIN
    INSERT INTO dbo.Employes
    (
        Nom,
        Prenom,
        Poste,
        NomUtilisateur,
        MotDePasse,
        IsManager,
        IsActif
    )
    VALUES
    (
        'MATALA',
        'MESSIE',
        'Superviseur',
        'MESSIE',
        '1234',
        1,
        1
    )
END-- Données initiales (admin, rôles)
