USE SalleDeSport;

-- 1. Admin Principal

CREATE USER 'AdminPrincipal'@'localhost' IDENTIFIED BY 'MdpAdminPrincipal';
GRANT ALL PRIVILEGES ON SalleDeSport.* TO 'AdminPrincipal'@'localhost';

-- 2. Admin Secondaire 

CREATE USER 'AdminSecondaire'@'localhost' IDENTIFIED BY 'MdpAdminSecondaire';
-- Il peut lire toutes les données
GRANT SELECT ON SalleDeSport.* TO 'AdminSecondaire'@'localhost';
-- Il peut gérer les utilisateurs, les coachs et les réservations
GRANT INSERT, UPDATE, DELETE ON SalleDeSport.Utilisateur TO 'AdminSecondaire'@'localhost';
GRANT INSERT, UPDATE, DELETE ON SalleDeSport.Coach TO 'AdminSecondaire'@'localhost';
GRANT INSERT, UPDATE, DELETE ON SalleDeSport.Reservation TO 'AdminSecondaire'@'localhost';
GRANT UPDATE ON SalleDeSport.Souscription TO 'AdminSecondaire'@'localhost';
-- (Il ne peut PAS supprimer de Salles ni changer les types d'adhésion)

-- 3. Membre

CREATE USER 'AppUser'@'localhost' IDENTIFIED BY 'MdpAppUser';
GRANT SELECT, INSERT, UPDATE, DELETE ON SalleDeSport.* TO 'AppUser'@'localhost';
GRANT SELECT ON SalleDeSport.TypeAdhesion TO 'AppUser'@'localhost';
GRANT INSERT, SELECT ON SalleDeSport.Souscription TO 'AppUser'@'localhost';

FLUSH PRIVILEGES; 