USE SalleDeSport;

-- 0. Désactivation temporaire des contraintes pour vider les tables proprement

SET FOREIGN_KEY_CHECKS = 0;
TRUNCATE TABLE Reservation;
TRUNCATE TABLE Souscription;
TRUNCATE TABLE Seance;
TRUNCATE TABLE Utilisateur;
TRUNCATE TABLE Salle;
TRUNCATE TABLE TypeCours;
TRUNCATE TABLE Coach;
TRUNCATE TABLE TypeAdhesion;
TRUNCATE TABLE Role;
SET FOREIGN_KEY_CHECKS = 1;

-- 1. Rôles 

INSERT INTO Role (IdRole, fonction) VALUES 
(1, 'Gérant'), 
(2, 'Staff'), 
(3, 'Membre');

-- 2. Types d'adhésion

INSERT INTO TypeAdhesion (IdTypeAdhesion, libelle, prix) VALUES 
(1, 'Standard', 29.99), 
(2, 'Premium', 39.99),
(3, 'Etudiant', 19.99);

-- 3. Salles 

INSERT INTO Salle (idSalle, nomSalle, capacite) VALUES 
(1, 'Studio Yoga', 10), 
(2, 'Salle Muscu', 30), 
(3, 'Salle Cardio', 30), 
(4, 'Salle CrossFit', 20);

-- 4. Types de Cours

INSERT INTO TypeCours (IdCours, NomCours, Description, Intensite) VALUES 
(1, 'Yoga', 'Yoga dynamique pour tous', 'Faible'), 
(2, 'Musculation', 'Renforcement musculaire avec poids', 'Moyenne'),
(3, 'Cardio', 'Optimisation de la VMA', 'Forte'),
(4, 'CrossFit', 'Entrainement intensif intégrale', 'Forte');

-- 5. Coachs

INSERT INTO Coach (IdCoach, nom, prenom, specialite, formation, telephone, email) VALUES 
(1, 'Ferrand', 'Hubert', 'Yoga', 'Master de Yoga', '0605862708', 'hubert.coach@gym.com'),
(2, 'Doat', 'Clément', 'Musculation', 'BPJEPS', '0601340304', 'clement.coach@gym.com'),
(3, 'Balamba', 'Cyrille', 'Cardio', 'STAPS', '0696887166', 'cyrille.coach@gym.com'),
(4, 'Inshape', 'Tibo', 'CrossFit', 'Master de Crossfit', '0676453426', 'tibo.coach@gym.com');

-- 6. Utilisateurs 

INSERT INTO Utilisateur (IdUtilisateur, nom, prenom, email, motDePasse, adresse, telephone, IdRole) VALUES 
-- Gérant
(1, 'Le-Grand', 'Chef', 'admin@gym.com', 'MdpAdmin', '12 rue de la paix','0129346205', 1), 
-- Membre
(2, 'Dupont', 'Jean', 'jean.dupont@mail.com', '123','12 rue de la paix','0621452729', 3), 
(3, 'Martin', 'Sophie', 'sophie.martin@mail.com', '123','12 rue de la paix','0763637337', 3), 
(4, 'Lefebvre', 'Thomas', 'thomas.le@mail.com', '123', '14 Rue de Rivoli', '0611000011', 3),
(5, 'Dubois', 'Manon', 'manon.du@mail.com', '123', '22 Av Foch', '0622000022', 3),
(6, 'Rousseau', 'Kevin', 'kevin.ro@mail.com', '123', '5 Rue du Port', '0633000033', 3),
(7, 'Vincent', 'Sarah', 'sarah.vi@mail.com', '123', '12 Rue Haute', '0644000044', 3),
(8, 'Fournier', 'Alex', 'alex.fo@mail.com', '123', '8 Blvd Sud', '0655000055', 3),
(9, 'Girard', 'Emma', 'emma.gi@mail.com', '123', '3 Impasse Verte', '0666000066', 3),
(10, 'Andre', 'Lucas', 'lucas.an@mail.com', '123', '9 Rue Bleue', '0677000077', 3),
(11, 'Mercier', 'Clara', 'clara.me@mail.com', '123', '17 Av des Pins', '0688000088', 3),
(12, 'Blanc', 'Hugo', 'hugo.bl@mail.com', '123', '2 Rue du Lac', '0699000099', 3),
(13, 'Guerin', 'Chloe', 'chloe.gu@mail.com', '123', '44 Rue du Parc', '0612121212', 3),
(14, 'Boyer', 'Enzo', 'enzo.bo@mail.com', '123', '1 Blvd de la Mer', '0623232323', 3),
(15, 'Garnier', 'Lea', 'lea.ga@mail.com', '123', '66 Rue des Arts', '0634343434', 3),
(16, 'Chevalier', 'Nathan', 'nathan.ch@mail.com', '123', '19 Rue Neuve', '0645454545', 3),
(17, 'Francois', 'Zoe', 'zoe.fr@mail.com', '123', '5 Rue de la Paix', '0656565656', 3),
(18, 'Fontaine', 'Leo', 'leo.fo@mail.com', '123', '33 Av Gambetta', '0667676767', 3),
(19, 'Morel', 'Jade', 'jade.mo@mail.com', '123', '25 Rue des Fleurs', '0678787878', 3),
(20, 'Fournier', 'Hugo', 'hugo.fo@mail.com', '123', '12 Blvd des Capucines', '0689898989', 3),
(21, 'Rousseau', 'Inès', 'ines.ro@mail.com', '123', '7 Av de la République', '0690909090', 3),
(22, 'Lambert', 'Mathis', 'mathis.la@mail.com', '123', '30 Rue de la Gare', '0610203040', 3),
(23, 'Bonnet', 'Léna', 'lena.bo@mail.com', '123', '15 Rue du Commerce', '0650607080', 3),
(24, 'Dupuis', 'Noé', 'noe.du@mail.com', '123', '8 Quai des Brumes', '0640302010', 3),
(25, 'Morin', 'Camille', 'camille.mo@mail.com', '123', '10 Rue de la Poste', '0611224455', 3),
(26, 'Nicolas', 'Sacha', 'sacha.ni@mail.com', '123', '21 Rue des Oliviers', '0677889900', 3),
(27, 'Petit', 'Alice', 'alice.pe@mail.com', '123', '5 Rue du Vallon', '0655443322', 3),
(28, 'Marchand', 'Louis', 'louis.ma@mail.com', '123', '14 Rue de l''Église', '0688776655', 3),
(29, 'Dumont', 'Clémence', 'clemence.du@mail.com', '123', '3 Blvd Voltaire', '0633221100', 3),
(30, 'Gauthier', 'Théo', 'theo.ga@mail.com', '123', '18 Rue de l''Avenir', '0612345678', 3),
-- Staff 
(31, 'Lemoine', 'Alice', 'alice.staff@gym.com', 'alice25', '8 Avenue des Arts', '0144556677', 2),
(32, 'Gerrard', 'Steve', 'steve.staff@gym.com', 'steve25', '3 bis Rue du Lac', '0612457801', 2),
(33, 'Vasseur', 'Julie', 'julie.staff@gym.com', 'julie25', '22 Boulevard Sud', '0789451230', 2),
(34, 'Moretti', 'Enzo', 'enzo.staff@gym.com', 'enzo25', '14 Impasse Verte', '0655443322', 2);

-- 7. Séances (Planning)

INSERT INTO Seance (IdSeance, DateDebut, DateFin, DureeMinutes, CapaciteMax, IdCours, IdCoach, idSalle) VALUES 
(1, '2026-01-01 10:00:00', '2026-01-01 11:30:00', 90, 5, 1, 1, 1),-- Yoga
(2, '2026-01-02 14:00:00', '2026-01-02 15:00:00', 60, 10, 4, 4, 4),-- CrossFit
(3, '2026-01-03 18:00:00', '2026-01-03 19:00:00', 60, 15, 2, 2, 2),-- Muscu
(4, '2026-01-08 10:00:00', '2026-01-08 11:00:00', 60, 15, 3, 3, 3), -- Cardio
(5, '2026-01-09 12:00:00', '2026-01-09 13:00:00', 60, 15, 2, 2, 2), -- Muscu
(6, '2026-01-10 09:30:00', '2026-01-10 10:30:00', 60, 5, 1, 1, 1), -- Yoga
(7, '2026-01-11 15:00:00', '2026-01-11 16:30:00', 90, 10, 4, 4, 4), -- CrossFit
(8, '2026-01-12 18:00:00', '2026-01-12 19:00:00', 60, 5, 1, 1, 1), -- Yoga
(9, '2026-01-13 08:00:00', '2026-01-13 09:00:00', 60, 15, 2, 2, 2), -- Muscu
(10, '2026-01-14 19:00:00', '2026-01-14 20:00:00', 60, 10, 4, 4, 4), -- CrossFit
(11, '2026-01-15 10:00:00', '2026-01-15 11:30:00', 90, 5, 1, 1, 1), -- Yoga
(12, '2026-01-16 14:00:00', '2026-01-16 15:00:00', 60, 15, 2, 2, 2), -- Muscu
(13, '2026-01-17 11:00:00', '2026-01-17 12:00:00', 60, 10, 4, 4, 4), -- CrossFit
(14, '2026-01-18 17:30:00', '2026-01-18 18:30:00', 60, 5, 1, 1, 1), -- Yoga
(15, '2026-01-19 09:00:00', '2026-01-19 10:30:00', 90, 15, 3, 3, 3), -- Cardio
(16, '2026-01-20 12:00:00', '2026-01-20 13:00:00', 60, 15, 2, 2, 2), -- Muscu
(17, '2026-01-21 18:00:00', '2026-01-21 19:00:00', 60, 10, 4, 4, 4), -- CrossFit
(18, '2026-01-22 19:30:00', '2026-01-22 20:30:00', 60, 5, 3, 3, 3); -- Cardio

-- 8. Souscriptions

INSERT INTO Souscription (dateDebut, dateFin, statut, IdUtilisateur, IdTypeAdhesion) VALUES
(CURDATE(), '2026-12-01', 'Validée', 2, 2),
(CURDATE(), '2026-12-01', 'Validée', 3, 1),
(CURDATE(), '2026-12-01', 'En attente', 5, 2),
(CURDATE(), '2026-12-01', 'Validée', 7, 3),
(CURDATE(), '2026-12-01','Validée' , 8, 2),
(CURDATE(), '2026-12-01', 'Validée', 10, 1),
(CURDATE(), '2026-12-01', 'En attente', 11, 2),
(CURDATE(), '2026-12-01', 'En attente', 12, 2),
(CURDATE(), '2026-12-01', 'Validée', 13, 3),
(CURDATE(), '2026-12-01', 'Validée', 14, 2),
(CURDATE(), '2026-12-01', 'Validée', 15, 1),
(CURDATE(), '2026-12-01', 'En attente', 16, 2),
(CURDATE(), '2026-12-01', 'Validée', 17, 1),
(CURDATE(), '2026-12-01', 'Validée', 18, 3),
(CURDATE(), '2026-12-01', 'En attente', 19, 2),
(CURDATE(), '2026-12-01', 'En attente', 20, 2),
(CURDATE(), '2026-12-01', 'Validée', 21, 3),
(CURDATE(), '2026-12-01', 'Validée', 22, 2),
(CURDATE(), '2026-12-01', 'Validée', 23, 1),
(CURDATE(), '2026-12-01', 'En attente', 24, 2),
(CURDATE(), '2026-12-01', 'Validée', 25, 1),
(CURDATE(), '2026-12-01', 'Validée', 27, 3),
(CURDATE(), '2026-12-01', 'En attente', 28, 2),
(CURDATE(), '2026-12-01', 'Validée', 30, 1);


-- 9. Réservations

INSERT INTO Reservation (IdUtilisateur, IdSeance, dateReservation) VALUES 
(2, 1, NOW()), (3, 1, NOW()),
(7, 2, NOW()), (13, 2, NOW()), (8, 2, NOW()), 
(14, 3, NOW()), (30, 3, NOW()), 
(10, 4, NOW()), (17, 4, NOW()), (27, 4, NOW()), 
(25, 5, NOW()), (15, 5, NOW()),            
(22, 6, NOW()), (2, 6, NOW()),   
(3, 7, NOW()), (21, 7, NOW()), (10, 7, NOW()),
(18, 8, NOW()), (21, 8, NOW()), (22, 8, NOW()), (7, 8, NOW()), (30, 8, NOW()), 
(15, 9, NOW()), (14, 9, NOW()), (23, 9, NOW()), 
(18, 10, NOW()), (22, 10, NOW()), (27, 10, NOW()), 
(10, 11, NOW()), (17, 11, NOW()), (2, 11, NOW()), (13, 11, NOW()), 
(14, 12, NOW()), (15, 12, NOW()), (7, 12, NOW()), 
(25, 13, NOW()), (18, 13, NOW()), (3, 13, NOW()), (30, 13, NOW()), 
(10, 14, NOW()), (17, 14, NOW()), (18, 14, NOW()), 
(13, 15, NOW()), (27, 15, NOW()), (10, 15, NOW()), 
(7, 16, NOW()), (15, 16, NOW()), (18, 16, NOW()), (14, 16, NOW()), 
(2, 17, NOW()), (3, 17, NOW()), (27, 17, NOW()), (13, 17, NOW()), 
(25, 18, NOW()), (14, 18, NOW()), (15, 18, NOW()); 