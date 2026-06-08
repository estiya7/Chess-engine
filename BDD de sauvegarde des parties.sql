USE Echecs;

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
                            Score FLOAT,
                            FOREIGN KEY (Programme_1) REFERENCES IA_versions (ID_version),
                            FOREIGN KEY (Programme_2) REFERENCES IA_versions (ID_version),
                            PRIMARY KEY (ID_Affrontement));
                            
CREATE TABLE Partie (ID_Partie INTEGER AUTO_INCREMENT,
					1v1 INTEGER,
                    Resultat FLOAT,
                    Coups VARCHAR(5000),
                    FOREIGN KEY (1v1) REFERENCES Affrontement (ID_Affrontement),
                    PRIMARY KEY (ID_Partie));
SELECT * FROM IA_versions;
SELECT * FROM Affrontement;
SELECT * FROM Partie;
                            