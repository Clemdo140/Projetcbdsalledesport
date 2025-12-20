USE SalleDeSport;

/* Authentification de l'utilisateur et récupération de son Rôle via une JOINTURE.
   Permet de diriger l'utilisateur vers le bon Menu (Admin, Staff ou Membre).
   UTILISATION : Méthode Authentification.Login() Appelé ligne 43 dans Program.cs
*/
SELECT u.*, r.fonction 
FROM Utilisateur u 
JOIN Role r ON u.IdRole = r.IdRole 
WHERE u.email = 'email_saisi' AND u.motDePasse = 'mdp_saisi';


/* Nombre total de membres actifs (rôle = 3).
   UTILISATION : MenuEvaluation()  
*/
SELECT COUNT(*) FROM Utilisateur WHERE IdRole = 3;

/* 
   Calcule le nombre de séances et les capacités minimum, maximum ainsi que la moyenne
   UTILISATION : Ligne 122 (MenuEvaluation)
*/
SELECT COUNT(IdSeance), MIN(CapaciteMax), MAX(CapaciteMax), AVG(CapaciteMax) 
FROM Seance;

/* 
   Renvoie tous les emails présents dans les deux tables tout en supprimant les doublons 
   UTILISATION : Ligne 133 (MenuEvaluation)
*/
SELECT email FROM Utilisateur 
UNION 
SELECT email FROM Coach;

/* 
   Détermine le taux d'occupation moyen des séances
   Calcul : (Nombre total de réservations * 100) / (Capacité totale offerte)
   UTILISATION : Ligne 137 (MenuEvaluation)
*/
SELECT (COUNT(R.IdReservation) * 100.0 / SUM(S.CapaciteMax)) 
FROM Seance S 
LEFT JOIN Reservation R ON S.IdSeance = R.IdSeance;

/* 
   Disponibilité des salles
   Permet de voir combien de cours sont prévus par salle. Cela affiche donc 0 si aucun cours n'est prévu
   UTILISATION : Ligne 147 (MenuEvaluation)
*/
SELECT Sa.nomSalle, COUNT(S.IdSeance) as NbCours 
FROM Seance S 
RIGHT JOIN Salle Sa ON S.idSalle = Sa.idSalle 
GROUP BY Sa.nomSalle;

/* Fais le TOP 3 des Coachs les plus populaires.

    JOIN : On relie les tables Coach et Seance vers Reservation pour savoir qui a réservé quel cours animé par quel coach.
    GROUP BY C.nom : A la place d'avoir une ligne par réservation, ça va écraser toutes les lignes qui concernet le même coach en une seule ligne unique.
      Cela crée alors des groupes (le groupe Clément, le groupe Hubert, le groupe Cyrille)
    COUNT(...) : Une fois les lignes regroupées, on compte combien il y avait de réservations dans chaque groupe.
    ORDER BY Nb DESC : On trie ces groupes du plus grand nombre au plus petit.
    LIMIT 3 : On coupe le résultat pour ne garder que les 3 premiers, ce qui correspond à l'affichage "Top 3" de l'application C#
    UTILISATION : Ligne 160 (MenuEvaluation)
*/
SELECT C.nom, COUNT(R.IdReservation) as Nb 
FROM Coach C 
JOIN Seance S ON C.IdCoach = S.IdCoach 
JOIN Reservation R ON S.IdSeance = R.IdSeance 
GROUP BY C.nom 
ORDER BY Nb DESC 
LIMIT 3;

/* Sélectionne le cours le plus populaire.
   Utilise un LEFT JOIN pour inclure même les cours sans réservation.
   UTILISATION : MenuEvaluation() 
*/
SELECT T.NomCours AS nomC, COUNT(R.IdReservation) AS NbParticipants
FROM TypeCours T
LEFT JOIN Seance S ON T.IdCours = S.IdCours
LEFT JOIN Reservation R ON S.IdSeance = R.IdSeance
GROUP BY T.NomCours;


/* 
   Affichage complet du planning pour le Gérant.
   INNER JOIN : Jointure entre Seance, TypeCours, Coach et Salle, et sous-requête pour compter les inscrits.
   Cette sous requête donne toutes les informations d'une séance à partir des ID stockés.
   SOUS-REQUÊTE SELECT COUNT :
   Pour CHAQUE ligne de séance affichée, SQL va exécuter une requête  qui compte le nombre de lignes dans la table 'Reservation' 
   correspondant à cet ID de séance précis (R.IdSeance = S.IdSeance).
   Cela permet d'avoir une colonne du nombre d'inscrits.
   ORDER BY : Tri chronologique pour une lecture logique du planning.
   UTILISATION : MenuAdminPrincipal, ligne 480
*/
SELECT 
    S.IdSeance, 
    T.NomCours AS nomC, 
    S.DateDebut, 
    S.CapaciteMax,
    C.prenom AS PrenomCoach, C.nom AS NomCoach,
    Sa.nomSalle,
    (SELECT COUNT(*) FROM Reservation R WHERE R.IdSeance = S.IdSeance) AS NbInscrits
FROM Seance S 
JOIN TypeCours T ON S.IdCours = T.IdCours
JOIN Coach C ON S.IdCoach = C.IdCoach
JOIN Salle Sa ON S.idSalle = Sa.idSalle
ORDER BY S.DateDebut ;

/* 
   Suppression d'un membre et nettoyage
   UTILISATION : MenuAdminPrincipal, ligne 310
*/
DELETE FROM Reservation WHERE IdUtilisateur = 1; 
DELETE FROM Souscription WHERE IdUtilisateur = 1;
DELETE FROM Utilisateur WHERE IdUtilisateur = 1;

/* 
   Ajout d'une nouvelle séance au planning.
   UTILISATION : MenuAdminPrincipal, ligne 553
*/
INSERT INTO Seance (IdCours, DateDebut, IdCoach, idSalle, CapaciteMax, DureeMinutes) 
VALUES (1, '2026-05-12 14:00:00', 2, 3, 20, 60);

/* 
   Suppression d'un cours
   Pour cela, il est nécessaire de supprimer les réservations associées d'abord
   UTILISATION : MenuAdminPrincipal -> Gestion Cours -> Supprimer , ligne 603
*/
DELETE FROM Reservation WHERE IdSeance = 1;
DELETE FROM Seance WHERE IdSeance = 1;

/* 
   Permet d’afficher tous les coachs, d’en ajouter un nouveau, et de supprimer un coach ainsi que toutes ses séances associées
   UTILISATION : MenuAdminPrincipal -> Gestion Coachs Ligne 395
*/

SELECT nom, prenom, specialite, formation, telephone, email FROM Coach;

INSERT INTO Coach (nom, prenom, specialite, formation, telephone, email) 
VALUES ('Nom', 'Prenom', 'Specialite', 'Formation', '0600000000', 'email@test.com');

DELETE FROM Seance WHERE IdCoach = 1;
DELETE FROM Coach WHERE IdCoach = 1;

/* Permet d’afficher toutes les salles, d’en ajouter une nouvelle, et de supprimer une salle ainsi que toutes les séances qui sont associées à cette salle
   UTILISATION : MenuAdminPrincipal -> Gestion Salles
*/

SELECT * FROM Salle; 

INSERT INTO Salle (nomSalle, capacite) VALUES ('Nom Salle', 20);

DELETE FROM Seance WHERE idSalle = 1;
DELETE FROM Salle WHERE idSalle = 1;




/* 
   Liste des membres avec leur état d'abonnement.
   Le LEFT JOIN permet de récupérer tous les utilisateurs (dans la table de gauche), même ceux qui n'ont aucune correspondance dans la table 'Souscription'
   UTILISATION : MenuAdminSecondaire, ligne 895
*/
SELECT U.nom, U.prenom, U.email, T.libelle as NomForfait, S.statut
FROM Utilisateur U
LEFT JOIN Souscription S ON U.IdUtilisateur = S.IdUtilisateur
LEFT JOIN TypeAdhesion T ON S.IdTypeAdhesion = T.IdTypeAdhesion
WHERE U.IdRole = 3;

-- ligne 1137, requête pour obtenir les réservations du membre
SELECT r.IdReservation, t.NomCours, s.DateDebut, sa.nomSalle 
FROM Reservation r 
JOIN Seance s ON r.IdSeance = s.IdSeance 
JOIN TypeCours t ON s.IdCours = t.IdCours 
JOIN Salle sa ON s.idSalle = sa.idSalle 
WHERE r.IdUtilisateur = 2 
ORDER BY s.DateDebut ASC;


/* 
   Validation d'une inscription.
   Permet de passer du statut de En attente à Validée.
   UTILISATION : ValiderInscription(), ligne 1295
*/
UPDATE Souscription SET statut = 'Validée' WHERE IdSouscription = 1;


/* 
   Permet de voir le planning détaillé pour le membre
   Récupère la description et l'intensité en plus des infos initiales
   UTILISATION : MenuMembre -> Voir Planning, ligne 631
*/
SELECT S.IdSeance, T.NomCours, T.Description, T.Intensite, S.DateDebut 
FROM Seance S 
JOIN TypeCours T ON S.IdCours = T.IdCours;

/* 
   Vérification de la disponibilité avant de réserver
   Cela vérifie si le membre a déjà réservé, c'est à dire s'il y a des doublons  ou non, et si le membre a un problème au niveau de l'horaire
   UTILISATION : MenuMembre, ligne 1042
*/
SELECT COUNT(*) FROM Reservation 
WHERE IdUtilisateur = 2 AND IdSeance = 5;

SELECT COUNT(*) FROM Reservation R
JOIN Seance S ON R.IdSeance = S.IdSeance
WHERE R.IdUtilisateur = 2 
AND S.DateDebut = '2026-01-01 10:00:00';

/* 
   Enregistrer la réservation après que les vérifications sont validées
   UTILISATION : MenuMembre, ligne 1056
*/
INSERT INTO Reservation (IdUtilisateur, IdSeance, dateReservation) 
VALUES (2, 5, NOW());



-- Ligne 1288, vérification de l'existence de l'inscription en attente
SELECT COUNT(*) FROM Souscription WHERE IdSouscription = 1 AND statut = 'En attente';


-- ligne 1270, requête pour obtenir les inscriptions en attente
SELECT s.IdSouscription, u.nom, u.prenom FROM Souscription s 
JOIN Utilisateur u ON s.IdUtilisateur = u.IdUtilisateur 
WHERE s.statut = 'En attente';




/* 
   Cette requête affiche les dates de début et fin, le type d’adhésion et le statut de toutes les souscriptions associées à un membre
   UTILISATION : MenuMembre -> Profil
*/
SELECT s.dateDebut, s.dateFin, t.libelle, s.statut 
FROM Souscription s 
JOIN TypeAdhesion t ON s.IdTypeAdhesion = t.IdTypeAdhesion 
WHERE s.IdUtilisateur = 2;

/* 
   Affichage du profil membre, c'est à dire son historique
   De plus, cela affiche les séances futures triées par date
   UTILISATION : MenuMembre, ligne 1096
*/
SELECT t.NomCours, s.DateDebut, sa.nomSalle 
FROM Reservation r 
JOIN Seance s ON r.IdSeance = s.IdSeance 
JOIN TypeCours t ON s.IdCours = t.IdCours 
JOIN Salle sa ON s.idSalle = sa.idSalle 
WHERE r.IdUtilisateur = 2 
ORDER BY s.DateDebut ASC;

-- Ligne 1163, vérification de l'existence de la réservatio
SELECT COUNT(*) FROM Reservation WHERE IdReservation = 1 AND IdUtilisateur = 2;


/* 
   Sert pour qu'un utilisateur puiise souscrire à un nouvel abonnement
   Supprime l'ancien abonnement avant d'en créer un nouveau
   UTILISATION : MenuMembre, ligne 1215
*/
DELETE FROM Souscription WHERE IdUtilisateur = 2;

-- Ligne 1221, requête pour insérer la nouvelle souscription
INSERT INTO Souscription (dateDebut, dateFin, statut, IdUtilisateur, IdTypeAdhesion) 
VALUES ('2025-01-01', '2026-01-01', 'En attente', 2, 1);

-- Ligne 1168, suppression de la réservation
DELETE FROM Reservation WHERE IdReservation = 1;



