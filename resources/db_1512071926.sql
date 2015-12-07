/*
SQLyog Community v12.14 (64 bit)
MySQL - 5.6.26-log : Database - is_dihub
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
/*Table structure for table `record` */

DROP TABLE IF EXISTS `record`;

CREATE TABLE `record` (
  `IdRecord` int(10) NOT NULL AUTO_INCREMENT COMMENT 'Autoincrementing primary key',
  `NodeId` int(10) NOT NULL COMMENT 'Id of the sensor',
  `Channel` varchar(1) NOT NULL COMMENT 'Channel of the sensor',
  `Value` float(6,4) NOT NULL DEFAULT '0.0000' COMMENT 'Value given by the sensor',
  `DateCreated` datetime NOT NULL COMMENT 'Datetime of the record',
  `DateCreatedTicks` bigint(20) NOT NULL COMMENT 'Datetime of the record represented by ammount of ticks',
  PRIMARY KEY (`IdRecord`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `record` */

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
