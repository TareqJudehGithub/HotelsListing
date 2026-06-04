import type { Country } from "../models/country";

import {
	Card,
	CardMedia,
	CardContent,
	Typography,
	CardActions,
	Button,
} from "@mui/material";

export default function Country({ country }: CountryProps) {
	return (
		<Card
			elevation={3}
			sx={{
				width: 280,
				borderRadius: 2,
				display: "flex",
				flexDirection: "column",
			}}
		>
			<CardMedia
				sx={{ height: 50, background: "cover" }}
				title={country.name}
			/>

			<CardContent
				sx={{
					textAlign: "center",
				}}
			>
				<Typography
					gutterBottom
					sx={{ textTransform: "uppercase", fontSize: "22px" }}
					variant="subtitle2"
				>
					{country.shortName} - {country.name}
				</Typography>
			</CardContent>
			<CardActions sx={{ justifyContent: "center" }}>
				<Button>View</Button>
			</CardActions>
		</Card>
	);
}

type CountryProps = {
	country: Country;
};
