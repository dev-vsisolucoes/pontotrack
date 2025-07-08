import React from "react";

import clsx from "clsx";
import { makeStyles } from "@material-ui/core/styles";
import Table from "@material-ui/core/Table";
import TableBody from "@material-ui/core/TableBody";
import TableCell from "@material-ui/core/TableCell";
import TableHead from "@material-ui/core/TableHead";
import TableRow from "@material-ui/core/TableRow";
import {
  Typography,
  ListItem,
  ListItemIcon,
  ListItemText,
  CssBaseline,
  Toolbar,
  AppBar,
  ListSubheader,
  IconButton,
  Badge,
  Drawer,
  Avatar,
  Divider,
  List,
  Container,
  Tooltip,
  TextField,
  FormControlLabel,
  Checkbox,
  Button,
  Box
} from "@material-ui/core";

import Grid from "@material-ui/core/Grid";
import Paper from "@material-ui/core/Paper";
import ChevronLeftIcon from "@material-ui/icons/ChevronLeft";
import NotificationsIcon from "@material-ui/icons/Notifications";
import ControlPointIcon from "@material-ui/icons/ControlPoint";
import DashboardIcon from "@material-ui/icons/Dashboard";
import ShoppingCartIcon from "@material-ui/icons/ShoppingCart";
import PeopleIcon from "@material-ui/icons/People";
import BarChartIcon from "@material-ui/icons/BarChart";
import LayersIcon from "@material-ui/icons/Layers";
import AssignmentIcon from "@material-ui/icons/Assignment";
import SettingsIcon from "@material-ui/icons/Settings";

import MenuIcon from "@material-ui/icons/Menu";

import { Link } from "react-router-dom";

//import Title from "./Title";

// function MainListItems() {
//   return (
//     <div>
//       <ListItem button component={Link} to="/main">
//         <ListItemIcon>
//           <DashboardIcon />
//         </ListItemIcon>
//         <ListItemText primary="Visão Geral" />
//       </ListItem>
//       <ListItem button>
//         <ListItemIcon>
//           <SettingsIcon />
//         </ListItemIcon>
//         <ListItemText primary="Configurações" />
//       </ListItem>
//       <ListItem button component={Link} to="/users">
//         <ListItemIcon>
//           <PeopleIcon />
//         </ListItemIcon>
//         <ListItemText primary="Usuários" />
//       </ListItem>
//       {/* <ListItem button>
//         <ListItemIcon>
//           <ShoppingCartIcon />
//         </ListItemIcon>
//         <ListItemText primary="Orders" />
//       </ListItem>
//       <ListItem button>
//         <ListItemIcon>
//           <BarChartIcon />
//         </ListItemIcon>
//         <ListItemText primary="Reports" />
//       </ListItem>
//       <ListItem button>
//         <ListItemIcon>
//           <LayersIcon />
//         </ListItemIcon>
//         <ListItemText primary="Integrations" />
//       </ListItem> */}
//     </div>
//   );
// }

// function SecondaryListItems() {
//   return (
//     <div>
//       <ListSubheader inset>Saved reports</ListSubheader>
//       <ListItem button>
//         <ListItemIcon>{/* <AssignmentIcon /> */}</ListItemIcon>
//         <ListItemText primary="Current month" />
//       </ListItem>
//       <ListItem button>
//         <ListItemIcon>{/* <AssignmentIcon /> */}</ListItemIcon>
//         <ListItemText primary="Last quarter" />
//       </ListItem>
//       <ListItem button>
//         <ListItemIcon>{/* <AssignmentIcon /> */}</ListItemIcon>
//         <ListItemText primary="Year-end sale" />
//       </ListItem>
//     </div>
//   );
// }

// const rows = [
//   createData(
//     0,
//     "16 Mar, 2019",
//     "Elvis Presley",
//     "Tupelo, MS",
//     "VISA ⠀•••• 3719",
//     312.44
//   ),
//   createData(
//     1,
//     "16 Mar, 2019",
//     "Paul McCartney",
//     "London, UK",
//     "VISA ⠀•••• 2574",
//     866.99
//   ),
//   createData(
//     2,
//     "16 Mar, 2019",
//     "Tom Scholz",
//     "Boston, MA",
//     "MC ⠀•••• 1253",
//     100.81
//   ),
//   createData(
//     3,
//     "16 Mar, 2019",
//     "Michael Jackson",
//     "Gary, IN",
//     "AMEX ⠀•••• 2000",
//     654.39
//   ),
//   createData(
//     4,
//     "15 Mar, 2019",
//     "Bruce Springsteen",
//     "Long Branch, NJ",
//     "VISA ⠀•••• 5919",
//     212.79
//   )
// ];

// function createData(id, date, name, shipTo, paymentMethod, amount) {
//   return { id, date, name, shipTo, paymentMethod, amount };
// }

// const drawerWidth = 240;

// const useStyles = makeStyles(theme => ({
//   root: {
//     display: "flex"
//   },
//   toolbar: {
//     paddingRight: 24 // keep right padding when drawer closed
//   },
//   toolbarIcon: {
//     display: "flex",
//     alignItems: "center",
//     justifyContent: "flex-end",
//     padding: "0 8px",
//     ...theme.mixins.toolbar
//   },
//   appBar: {
//     zIndex: theme.zIndex.drawer + 1,
//     transition: theme.transitions.create(["width", "margin"], {
//       easing: theme.transitions.easing.sharp,
//       duration: theme.transitions.duration.leavingScreen
//     })
//   },
//   appBarShift: {
//     marginLeft: drawerWidth,
//     width: `calc(100% - ${drawerWidth}px)`,
//     transition: theme.transitions.create(["width", "margin"], {
//       easing: theme.transitions.easing.sharp,
//       duration: theme.transitions.duration.enteringScreen
//     })
//   },
//   menuButton: {
//     marginRight: 36
//   },
//   menuButtonHidden: {
//     display: "none"
//   },
//   title: {
//     flexGrow: 1,
//     marginLeft: theme.spacing(-2)
//   },
//   drawerPaper: {
//     position: "relative",
//     whiteSpace: "nowrap",
//     width: drawerWidth,
//     transition: theme.transitions.create("width", {
//       easing: theme.transitions.easing.sharp,
//       duration: theme.transitions.duration.enteringScreen
//     })
//   },
//   drawerPaperClose: {
//     overflowX: "hidden",
//     transition: theme.transitions.create("width", {
//       easing: theme.transitions.easing.sharp,
//       duration: theme.transitions.duration.leavingScreen
//     }),
//     width: theme.spacing(7),
//     [theme.breakpoints.up("sm")]: {
//       width: theme.spacing(9)
//     }
//   },
//   appBarSpacer: theme.mixins.toolbar,
//   content: {
//     flexGrow: 1,
//     height: "100vh",
//     overflow: "auto"
//   },
//   container: {
//     paddingTop: theme.spacing(4),
//     paddingBottom: theme.spacing(4)
//   },
//   paper: {
//     padding: theme.spacing(2),
//     display: "flex",
//     overflow: "auto",
//     flexDirection: "column"
//   },
//   fixedHeight: {
//     height: 240
//   },
//   avatar: {
//     alignItems: "center",
//     marginTop: theme.spacing(1),
//     marginLeft: theme.spacing(1),
//     marginBottom: theme.spacing(2)
//   },

//   addButton: {
//     marginRight: theme.spacing(-3)
//   },
//   submit: {
//     margin: theme.spacing(3, 1, 2)
//   }
// }));

import useStyles from "../theme";

export default function VisaoGeral(props) {
  const classes = useStyles();

  return (
    <div>
      <Paper className={classes.paper}>
        <Toolbar>
          <Typography
            className={classes.title}
            variant="h6"
            id="tableTitle"
            component="div"
            color="primary"
          >
            {props.title}
          </Typography>
        </Toolbar>
      </Paper>
    </div>
  );
}
