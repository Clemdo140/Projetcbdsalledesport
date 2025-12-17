CREATE DATABASE SalleDeSport;
USE SalleDeSport;

CREATE TABLE Role ( IdRole INT PRIMARY KEY AUTO_INCREMENT,
					fonction VARCHAR(50) NOT NULL);

CREATE TABLE TypeAdhesion ( IdTypeAdhesion INT PRIMARY KEY AUTO_INCREMENT,
							libelle VARCHAR(50) NOT NULL,
                            prix DECIMAL(10, 2) NOT NULL);
                            
                            
CREATE TABLE Coach ( IdCoach INT PRIMARY KEY AUTO_INCREMENT,
					 nom VARCHAR(50) NOT NULL,
                     prenom VARCHAR(50) NOT NULL,
                     specialite VARCHAR(100),
                     formation VARCHAR(100),
                     telephone VARCHAR(15),
                     email VARCHAR(100) );
                     
CREATE TABLE TypeCours ( IdCours INT PRIMARY KEY AUTO_INCREMENT,
						 NomCours VARCHAR(50) NOT NULL,
                         Description VARCHAR(100),
                         Intensite VARCHAR(20) );
                         
CREATE TABLE Salle ( idSalle INT PRIMARY KEY AUTO_INCREMENT,
					 nomSalle VARCHAR(50) NOT NULL,
                     capacite INT NOT NULL );
                     
CREATE TABLE Utilisateur ( IdUtilisateur INT PRIMARY KEY AUTO_INCREMENT,
						   nom VARCHAR(50) NOT NULL,
                           prenom VARCHAR(50) NOT NULL,
                           email VARCHAR(100) NOT NULL UNIQUE,
                           motDePasse VARCHAR(255) NOT NULL,
                           adresse VARCHAR(100),
                           telephone VARCHAR(15),
                           IdRole INT NOT NULL,
                           FOREIGN KEY(IdRole) REFERENCES Role(IdRole) );
                           
CREATE TABLE Seance ( IdSeance INT PRIMARY KEY AUTO_INCREMENT,
					  DateDebut DATETIME NOT NULL,
                      DateFin DATETIME,
                      DureeMinutes INT NOT NULL,
                      CapaciteMax INT NOT NULL,
                      IdCours INT NOT NULL,
                      IdCoach INT NOT NULL,
                      idSalle INT NOT NULL,
                      FOREIGN KEY(IdCours) REFERENCES TypeCours(IdCours),
                      FOREIGN KEY(IdCoach) REFERENCES Coach(IdCoach),
                      FOREIGN KEY(idSalle) REFERENCES Salle(idSalle)
);

CREATE TABLE Souscription ( IdSouscription INT PRIMARY KEY AUTO_INCREMENT,
							dateDebut DATE NOT NULL,
                            dateFin DATE NOT NULL,
                            statut VARCHAR(20) NOT NULL,
                            IdUtilisateur INT NOT NULL,
                            IdTypeAdhesion INT NOT NULL,
                            FOREIGN KEY(IdUtilisateur) REFERENCES Utilisateur(IdUtilisateur),
                            FOREIGN KEY(IdTypeAdhesion) REFERENCES TypeAdhesion(IdTypeAdhesion) );
						
CREATE TABLE Reservation ( IdReservation INT PRIMARY KEY AUTO_INCREMENT,
						   dateReservation DATETIME DEFAULT NOW(),
                           IdUtilisateur INT NOT NULL,
                           IdSeance INT NOT NULL,
                           FOREIGN KEY(IdUtilisateur) REFERENCES Utilisateur(IdUtilisateur),
                           FOREIGN KEY(IdSeance) REFERENCES Seance(IdSeance) );
                           
ALTER TABLE TypeAdhesion
ADD CONSTRAINT CPrixAdhesion_Positif CHECK (prix >= 0);


ALTER TABLE Seance
ADD CONSTRAINT DureeSeance_Positive CHECK (DureeMinutes > 0);

ALTER TABLE Souscription
ADD CONSTRAINT DateSouscription_Logique CHECK (dateFin >= dateDebut);


                           

                            


                           
                           
