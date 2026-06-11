USE Echecs;

-- Script de création des tables
/*
DROP TABLE IF EXISTS Partie;
DROP TABLE IF EXISTS Affrontement;
DROP TABLE IF EXISTS IA_versions;

CREATE TABLE IA_versions (ID_version INTEGER AUTO_INCREMENT,
							Nom VARCHAR(40),
							Elo INTEGER CHECK (Elo >= 100),
							PRIMARY KEY (ID_version));
                            
CREATE TABLE Affrontement (ID_Affrontement INTEGER AUTO_INCREMENT,
							Programme_1 INTEGER,
							Programme_2 INTEGER,
                            Nombre_parties INTEGER,
                            Victoires_prgm_1 INTEGER,
                            Nulles INTEGER,
                            Victoires_prgm_2 INTEGER,
                            Score FLOAT,
                            FOREIGN KEY (Programme_1) REFERENCES IA_versions (ID_version),
                            FOREIGN KEY (Programme_2) REFERENCES IA_versions (ID_version),
                            PRIMARY KEY (ID_Affrontement));
                            
CREATE TABLE Partie (ID_Partie INTEGER AUTO_INCREMENT,
					id_affrontement INTEGER,
                    Ordre_partie INTEGER,
                    Resultat FLOAT,
                    Coups VARCHAR(5000),
                    FOREIGN KEY (id_affrontement) REFERENCES Affrontement (ID_Affrontement),
                    PRIMARY KEY (ID_Partie));
*/
-- DELETE FROM Partie WHERE id_affrontement = 1;
-- DELETE FROM Affrontement WHERE ID_Affrontement = 1;
SELECT * FROM IA_versions;
SELECT * FROM Affrontement;
SELECT * FROM Partie;

SELECT MAX(ID_Affrontement) FROM Affrontement;