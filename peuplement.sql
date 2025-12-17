USE SalleDeSport;

-- 1. On crée les rôles indispensables
INSERT INTO Role (IdRole, fonction) VALUES (1, 'Gérant'), (2, 'Staff'), (3, 'Membre');

-- 2. On crée ton utilisateur de test
-- Email : admin@test.fr | Mot de passe : 1234
INSERT INTO Utilisateur (nom, prenom, email, motDePasse, IdRole) 
VALUES ('NOM_GERANT', 'PRENOM_GERANT', 'admin@test.fr', '1234', 1);