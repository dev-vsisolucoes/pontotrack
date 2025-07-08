import React from "react";

import { Snackbar, Fade, IconButton } from "@material-ui/core";

import CloseIcon from '@material-ui/icons/Close';

export default function KSnackBar(props) {
    return (
        <Snackbar
            open={props.options[0].open}
            anchorOrigin={{
                vertical: 'top',
                horizontal: 'center',
            }}
            onClose={(event) => {
                props.options[1]({
                    open: false,
                    message: ""
                });
            }}
            TransitionComponent={Fade}
            message={props.options[0].message}
            autoHideDuration={3000}
            action={
                <React.Fragment>
                    <IconButton
                        size="small"
                        aria-label="close"
                        color="inherit"
                        onClick={
                            (event) => {
                                props.options[1]({
                                    open: false,
                                    message: ""
                                });
                            }
                        }
                    >
                        <CloseIcon fontSize="small" />
                    </IconButton>
                </React.Fragment>
            }
        />
    )
}