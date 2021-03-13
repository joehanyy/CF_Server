/*
Navicat MySQL Data Transfer

Source Server         : localhost
Source Server Version : 50051
Source Host           : localhost:3306
Source Database       : cf

Target Server Type    : MYSQL
Target Server Version : 50051
File Encoding         : 65001

Date: 2017-07-22 03:46:17
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for accounts
-- ----------------------------
DROP TABLE IF EXISTS `accounts`;
CREATE TABLE `accounts` (
  `Username` varchar(12) NOT NULL default '',
  `Password` varchar(17) NOT NULL default '',
  `IP` varchar(16) default NULL,
  `State` tinyint(5) unsigned zerofill default NULL,
  `EntityID` bigint(18) unsigned default '0',
  `Email` varchar(100) NOT NULL default '',
  `Question` varchar(255) NOT NULL,
  `Answer` varchar(255) NOT NULL,
  `City` varchar(100) default NULL,
  `MobileNumber` varchar(100) NOT NULL,
  `SecretQuestion` varchar(100) NOT NULL,
  PRIMARY KEY  (`Username`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of accounts
-- ----------------------------
INSERT INTO `accounts` VALUES ('1', '1', '25.45.123.240', '00100', '20000000', '', '', '', null, '', '');

-- ----------------------------
-- Table structure for characters
-- ----------------------------
DROP TABLE IF EXISTS `characters`;
CREATE TABLE `characters` (
  `UID` bigint(255) unsigned NOT NULL default '0',
  `Name` varchar(12) NOT NULL,
  `Clan` varchar(255) NOT NULL,
  `Experience` bigint(255) unsigned NOT NULL default '0',
  `GP` bigint(255) unsigned NOT NULL default '0',
  `ZP` bigint(255) unsigned NOT NULL default '0',
  `HeadshotKills` bigint(255) unsigned NOT NULL default '0',
  `KnifeKills` bigint(255) unsigned NOT NULL default '0',
  `GeneradeKills` bigint(255) unsigned NOT NULL default '0',
  `Deserion` bigint(255) unsigned NOT NULL default '0',
  `TeamKills` bigint(255) unsigned NOT NULL default '0',
  `Battles` longblob NOT NULL,
  `VIP` bigint(255) unsigned NOT NULL default '0',
  PRIMARY KEY  (`UID`,`Name`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of characters
-- ----------------------------
INSERT INTO `characters` VALUES ('20000000', '1stTest', 'pineAppleXpresS', '25363461', '10000000', '10000', '10000', '500', '1500', '0', '500', 0x00000000, '0');
INSERT INTO `characters` VALUES ('20000001', '2ndTest', '', '0', '0', '0', '0', '0', '0', '0', '0', '', '0');
SET FOREIGN_KEY_CHECKS=1;
