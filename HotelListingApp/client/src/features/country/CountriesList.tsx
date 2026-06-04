import { useState, useEffect } from "react";
import type { Country as CountryModel } from "../models/country";
import Country from "./Country";
import { Box, Typography } from "@mui/material";

export default function CountriesList() {
	// // States
	const [countries, setCountries] = useState<CountryModel[]>([]);

	useEffect(() => {
		const url: string = "https://localhost:5001/api/v1/countries";

		const fetchData = async () => {
			const response = await fetch(url);
			const data = await response.json();

			setCountries(data);
		};
		fetchData();
	}, []);

	return (
		<Box
			sx={{
				display: "flex",
				flexWrap: "wrap",
				gap: 3,
				justifyContent: "center",
			}}
		>
			<Typography variant="h3">Countries List</Typography>
			{countries.map((country) => (
				<Country key={country.id} country={country} />
			))}
		</Box>
	);
}
