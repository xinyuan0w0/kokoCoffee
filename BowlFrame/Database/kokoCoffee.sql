/*
 Navicat Premium Data Transfer

 Source Server         : Mysql_80
 Source Server Type    : MySQL
 Source Server Version : 80035 (8.0.35)
 Source Host           : localhost:3306
 Source Schema         : kokobot_test

 Target Server Type    : MySQL
 Target Server Version : 80035 (8.0.35)
 File Encoding         : 65001

 Date: 29/07/2024 12:45:13
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for audit
-- ----------------------------
DROP TABLE IF EXISTS `audit`;
CREATE TABLE `audit`  (
  `Form` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Action` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UUID` varbinary(36) NOT NULL,
  `Timestamp` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Data` json NULL
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for data
-- ----------------------------
DROP TABLE IF EXISTS `data`;
CREATE TABLE `data`  (
  `UUID` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Key` tinytext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '主Key',
  `SubKey` tinytext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '子Key',
  `DataType` tinyint NOT NULL COMMENT '数据类型 0 Null 1 长整数 2 小数 3 布尔 4 文本 5 对象 6 数组 7 日期',
  `Value` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  INDEX `Data_Link_ID`(`UUID` ASC) USING BTREE,
  CONSTRAINT `Data_Link_ID` FOREIGN KEY (`UUID`) REFERENCES `id` (`UUID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for id
-- ----------------------------
DROP TABLE IF EXISTS `id`;
CREATE TABLE `id`  (
  `UID` bigint NOT NULL AUTO_INCREMENT,
  `UUID` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Type` int NOT NULL COMMENT '0 Groups 1 User 2 Group 3 Channel 4 SubChannel 5 Func',
  PRIMARY KEY (`UID`, `UUID`) USING BTREE,
  UNIQUE INDEX `User_Index`(`UID` ASC, `UUID` ASC) USING BTREE,
  INDEX `UUID`(`UUID` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 10983 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for perms
-- ----------------------------
DROP TABLE IF EXISTS `perms`;
CREATE TABLE `perms`  (
  `ID` int NOT NULL AUTO_INCREMENT,
  `UUID` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Permission` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` bit(1) NOT NULL DEFAULT b'1',
  `Area` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '全局 Global 私聊 Private 群聊 Group 分平台 使用数组',
  `Expir` bigint NULL DEFAULT 0,
  `Content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL COMMENT '额外参数',
  PRIMARY KEY (`ID`) USING BTREE,
  INDEX `Perms_Link_ID`(`UUID` ASC) USING BTREE,
  CONSTRAINT `Perms_Link_ID` FOREIGN KEY (`UUID`) REFERENCES `id` (`UUID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 945 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for platformid
-- ----------------------------
DROP TABLE IF EXISTS `platformid`;
CREATE TABLE `platformid`  (
  `UUID` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Platform` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `FatherUUID` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `ID` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  INDEX `PlatformID_Link_ID_2`(`FatherUUID` ASC) USING BTREE,
  INDEX `PlatformID_Link_ID_1`(`UUID` ASC) USING BTREE,
  CONSTRAINT `PlatformID_Link_ID_1` FOREIGN KEY (`UUID`) REFERENCES `id` (`UUID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `PlatformID_Link_ID_2` FOREIGN KEY (`FatherUUID`) REFERENCES `id` (`UUID`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Triggers structure for table data
-- ----------------------------
DROP TRIGGER IF EXISTS `Data_Audit_Add`;
delimiter ;;
CREATE TRIGGER `Data_Audit_Add` AFTER INSERT ON `data` FOR EACH ROW Insert Into audit (`Form`,`Action`,`UUID`,`Data`) Values('data','ADD_NEW_DATA',new.UUID,JSON_OBJECT('Key',new.Key,'SubKey',new.SubKey,'DataType',new.DataType,'Value',new.Value))
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table data
-- ----------------------------
DROP TRIGGER IF EXISTS `Data_Audit_Update`;
delimiter ;;
CREATE TRIGGER `Data_Audit_Update` AFTER UPDATE ON `data` FOR EACH ROW Insert Into audit (`Form`,`Action`,`UUID`,`Data`) Values('data','UPDATE_DATA',old.UUID,JSON_OBJECT('UUID',new.UUID,'Key',new.Key,'SubKey',new.SubKey,'DataType',new.DataType,'Value',new.Value))
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table data
-- ----------------------------
DROP TRIGGER IF EXISTS `Data_Audit_Del`;
delimiter ;;
CREATE TRIGGER `Data_Audit_Del` AFTER DELETE ON `data` FOR EACH ROW Insert Into audit (`Form`,`Action`,`UUID`,`Data`) Values('data','DEL_DATA',old.UUID,JSON_OBJECT('Key',old.Key,'SubKey',old.SubKey,'DataType',old.DataType,'Value',old.Value))
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table id
-- ----------------------------
DROP TRIGGER IF EXISTS `ID_Audit_Add`;
delimiter ;;
CREATE TRIGGER `ID_Audit_Add` AFTER INSERT ON `id` FOR EACH ROW Insert Into audit (`Form`,`Action`,`UUID`,`Data`) Values('id','ADD_NEW_ID',new.UUID,JSON_OBJECT('UID',new.UID,'Type',new.Type))
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table id
-- ----------------------------
DROP TRIGGER IF EXISTS `ID_Audit_Update`;
delimiter ;;
CREATE TRIGGER `ID_Audit_Update` AFTER UPDATE ON `id` FOR EACH ROW Insert Into audit (`Form`,`Action`,`UUID`,`Data`) Values('id','UPDATE_ID',old.UUID,JSON_OBJECT('UID',new.UID,'UUID',new.UUID,'Type',new.Type))
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table id
-- ----------------------------
DROP TRIGGER IF EXISTS `ID_Audit_Delete`;
delimiter ;;
CREATE TRIGGER `ID_Audit_Delete` AFTER DELETE ON `id` FOR EACH ROW Insert Into audit (`Form`,`Action`,`UUID`,`Data`) Values('id','DELETE_ID',old.UUID,JSON_OBJECT('UID',old.UID,'UUID',old.UUID,'Type',old.Type))
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table perms
-- ----------------------------
DROP TRIGGER IF EXISTS `Perm_Audit_ADD`;
delimiter ;;
CREATE TRIGGER `Perm_Audit_ADD` AFTER INSERT ON `perms` FOR EACH ROW Insert Into audit (`Form`,`Action`,`UUID`,`Data`) Values('perms','ADD_NEW_PERMISSION',new.UUID,JSON_OBJECT('ID',new.ID,'Permission',new.Permission,'Value',new.Value,'Area',new.Area,'Expir',new.Expir,'Content',new.Content))
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table perms
-- ----------------------------
DROP TRIGGER IF EXISTS `Perm_Audit_Update`;
delimiter ;;
CREATE TRIGGER `Perm_Audit_Update` AFTER UPDATE ON `perms` FOR EACH ROW Insert Into audit (`Form`,`Action`,`UUID`,`Data`) Values('perms','UPDATE_PERMISSION',old.UUID,JSON_OBJECT('ID',new.ID,'UUID',new.UUID,'Permission',new.Permission,'Value',new.Value,'Area',new.Area,'Expir',new.Expir,'Content',new.Content))
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table perms
-- ----------------------------
DROP TRIGGER IF EXISTS `Perm_Audit_Del`;
delimiter ;;
CREATE TRIGGER `Perm_Audit_Del` AFTER DELETE ON `perms` FOR EACH ROW Insert Into audit (`Form`,`Action`,`UUID`,`Data`) Values('perms','DEL_PERMISSION',old.UUID,JSON_OBJECT('ID',old.ID,'Permission',old.Permission,'Value',old.Value,'Area',old.Area,'Expir',old.Expir,'Content',old.Content))
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table platformid
-- ----------------------------
DROP TRIGGER IF EXISTS `PlatformID_Audit_Add`;
delimiter ;;
CREATE TRIGGER `PlatformID_Audit_Add` AFTER INSERT ON `platformid` FOR EACH ROW Insert Into audit (`Form`,`Action`,`UUID`,`Data`) Values('platformid','ADD_NEW_PLATFORMID',new.UUID,JSON_OBJECT('Platform',new.Platform,'FatherUUID',new.FatherUUID,'ID',new.ID))
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table platformid
-- ----------------------------
DROP TRIGGER IF EXISTS `PlatformID_Audit_Update`;
delimiter ;;
CREATE TRIGGER `PlatformID_Audit_Update` AFTER UPDATE ON `platformid` FOR EACH ROW Insert Into audit (`Form`,`Action`,`UUID`,`Data`) Values('platformid','UPDATE_PLATFORMID',old.UUID,JSON_OBJECT('UUID',old.UUID,'Platform',new.Platform,'FatherUUID',new.FatherUUID,'ID',new.ID))
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table platformid
-- ----------------------------
DROP TRIGGER IF EXISTS `PlatformID_Audit_Del`;
delimiter ;;
CREATE TRIGGER `PlatformID_Audit_Del` AFTER DELETE ON `platformid` FOR EACH ROW Insert Into audit (`Form`,`Action`,`UUID`,`Data`) Values('platformid','DEL_PLATFORMID',old.UUID,JSON_OBJECT('Platform',old.Platform,'FatherUUID',old.FatherUUID,'ID',old.ID))
;;
delimiter ;

SET FOREIGN_KEY_CHECKS = 1;
